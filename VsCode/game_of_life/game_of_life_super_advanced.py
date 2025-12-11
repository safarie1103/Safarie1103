import pygame
import random
import json
import os

# ----- CONFIG -----
WINDOW_WIDTH = 1000
WINDOW_HEIGHT = 700

GRID_WIDTH = 200          # world cells horizontally
GRID_HEIGHT = 200         # world cells vertically

BASE_CELL_SIZE = 10       # starting pixel size per cell
MIN_CELL_SIZE = 4
MAX_CELL_SIZE = 40

INITIAL_SPEED = 10        # generations per second (when running)
ALIVE_COLOR = (0, 255, 0)
DEAD_COLOR = (0, 0, 0)
GRID_COLOR = (40, 40, 40)
TEXT_COLOR = (200, 200, 200)
BG_TEXT_COLOR = (0, 0, 0)

PATTERN_FILE = "pattern.json"
# -------------------


def create_grid(randomize=False):
    '''Create grids for cell state and age.'''
    grid = []
    age_grid = []
    for _ in range(GRID_HEIGHT):
        row = []
        age_row = []
        for _ in range(GRID_WIDTH):
            if randomize and random.random() < 0.2:
                row.append(1)
                age_row.append(1)
            else:
                row.append(0)
                age_row.append(0)
        grid.append(row)
        age_grid.append(age_row)
    return grid, age_grid


def count_neighbors(grid, x, y, wrap_edges=False):
    '''Count live neighbors around cell (x, y).'''
    neighbors = 0
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            if dx == 0 and dy == 0:
                continue
            nx, ny = x + dx, y + dy
            if wrap_edges:
                nx %= GRID_WIDTH
                ny %= GRID_HEIGHT
                neighbors += grid[ny][nx]
            else:
                if 0 <= nx < GRID_WIDTH and 0 <= ny < GRID_HEIGHT:
                    neighbors += grid[ny][nx]
    return neighbors


def step(grid, age_grid, wrap_edges=False):
    '''Compute the next generation and update ages.'''
    new_grid = [[0 for _ in range(GRID_WIDTH)] for _ in range(GRID_HEIGHT)]
    new_ages = [[0 for _ in range(GRID_WIDTH)] for _ in range(GRID_HEIGHT)]
    for y in range(GRID_HEIGHT):
        for x in range(GRID_WIDTH):
            alive = grid[y][x] == 1
            neighbors = count_neighbors(grid, x, y, wrap_edges=wrap_edges)

            if alive:
                if neighbors == 2 or neighbors == 3:
                    new_grid[y][x] = 1
                    new_ages[y][x] = age_grid[y][x] + 1
                else:
                    new_grid[y][x] = 0
                    new_ages[y][x] = 0
            else:
                if neighbors == 3:
                    new_grid[y][x] = 1
                    new_ages[y][x] = 1
                else:
                    new_grid[y][x] = 0
                    new_ages[y][x] = 0
    return new_grid, new_ages


def age_to_color(age):
    '''Map cell age to a color (young = green, old = white).'''
    if age <= 0:
        return ALIVE_COLOR
    max_age = 10
    t = min(age, max_age) / max_age
    r = int(ALIVE_COLOR[0] * (1 - t) + 255 * t)
    g = int(ALIVE_COLOR[1] * (1 - t) + 255 * t)
    b = int(ALIVE_COLOR[2] * (1 - t) + 255 * t)
    return (r, g, b)


def draw_grid(screen, grid, age_grid, cell_size, camera_x, camera_y):
    '''Draw the visible part of the grid and the cells.'''
    screen.fill(DEAD_COLOR)

    cols_visible = WINDOW_WIDTH // cell_size + 1
    rows_visible = WINDOW_HEIGHT // cell_size + 1

    for sy in range(rows_visible):
        gy = camera_y + sy
        if not (0 <= gy < GRID_HEIGHT):
            continue
        for sx in range(cols_visible):
            gx = camera_x + sx
            if not (0 <= gx < GRID_WIDTH):
                continue
            if grid[gy][gx] == 1:
                rect = pygame.Rect(
                    sx * cell_size,
                    sy * cell_size,
                    cell_size,
                    cell_size,
                )
                color = age_to_color(age_grid[gy][gx])
                pygame.draw.rect(screen, color, rect)

    # Draw grid lines (only for visible area)
    for sx in range(cols_visible + 1):
        pygame.draw.line(
            screen,
            GRID_COLOR,
            (sx * cell_size, 0),
            (sx * cell_size, WINDOW_HEIGHT),
            1,
        )
    for sy in range(rows_visible + 1):
        pygame.draw.line(
            screen,
            GRID_COLOR,
            (0, sy * cell_size),
            (WINDOW_WIDTH, sy * cell_size),
            1,
        )


def draw_status(screen, font, generation, speed, paused, wrap_edges, cell_size, camera_x, camera_y):
    '''Draw status text (generation, speed, controls).'''
    lines = [
        f"Gen: {generation}   Speed: {speed} gen/s   "
        f"{'PAUSED' if paused else 'RUNNING'}   "
        f"Wrap: {'ON' if wrap_edges else 'OFF'}   "
        f"Zoom: {cell_size}px   Camera: ({camera_x}, {camera_y})",
        "SPACE: pause/run   N: step   R: random   C: clear   T: toggle wrap",
        "I/J/K/L: pan camera   Q/E: zoom +/-   UP/DOWN: speed +/-",
        "1: glider   2: small exploder   F5: save pattern   F9: load pattern",
        "Click (when paused) to toggle cells.",
    ]

    padding = 4
    line_height = font.get_linesize()
    box_height = line_height * len(lines) + padding * 2
    box_rect = pygame.Rect(0, 0, WINDOW_WIDTH, box_height)
    pygame.draw.rect(screen, BG_TEXT_COLOR, box_rect)

    y = padding
    for line in lines:
        surf = font.render(line, True, TEXT_COLOR)
        screen.blit(surf, (padding, y))
        y += line_height


# --- Patterns ---

GLIDER = [
    (1, 0),
    (2, 1),
    (0, 2), (1, 2), (2, 2),
]

SMALL_EXPLODER = [
            (0, 1),
    (-1, 0), (0, 0), (1, 0),
            (-1, -1), (1, -1),
            (0, -2),
]


def stamp_pattern(grid, age_grid, x, y, pattern):
    '''Stamp a pattern (list of (dx, dy)) with origin at (x, y).'''
    for dx, dy in pattern:
        gx = x + dx
        gy = y + dy
        if 0 <= gx < GRID_WIDTH and 0 <= gy < GRID_HEIGHT:
            grid[gy][gx] = 1
            age_grid[gy][gx] = 1


def toggle_cell(grid, age_grid, gx, gy):
    '''Toggle a single cell and its age.'''
    if 0 <= gx < GRID_WIDTH and 0 <= gy < GRID_HEIGHT:
        if grid[gy][gx] == 1:
            grid[gy][gx] = 0
            age_grid[gy][gx] = 0
        else:
            grid[gy][gx] = 1
            age_grid[gy][gx] = 1


def save_pattern(grid, filename=PATTERN_FILE):
    try:
        with open(filename, "w", encoding="utf-8") as f:
            json.dump(grid, f)
        print(f"Saved pattern to {os.path.abspath(filename)}")
    except Exception as e:
        print("Error saving pattern:", e)


def load_pattern(filename=PATTERN_FILE):
    try:
        with open(filename, "r", encoding="utf-8") as f:
            data = json.load(f)
        # Validate size
        if len(data) != GRID_HEIGHT or any(len(row) != GRID_WIDTH for row in data):
            print("Pattern size does not match grid; resizing not supported.")
            return None
        print(f"Loaded pattern from {os.path.abspath(filename)}")
        return data
    except FileNotFoundError:
        print("Pattern file not found:", filename)
    except Exception as e:
        print("Error loading pattern:", e)
    return None


def main():
    pygame.init()
    screen = pygame.display.set_mode((WINDOW_WIDTH, WINDOW_HEIGHT))
    pygame.display.set_caption("Conway's Game of Life - Super Advanced")

    clock = pygame.time.Clock()
    font = pygame.font.SysFont("consolas", 16)

    grid, age_grid = create_grid(randomize=False)
    running = True
    paused = True  # start paused so you can draw
    wrap_edges = False
    generation = 0

    speed = INITIAL_SPEED  # generations per second, 1..60
    cell_size = BASE_CELL_SIZE

    camera_x = 0
    camera_y = 0

    time_accumulator = 0.0

    while running:
        dt = clock.tick(60) / 1000.0  # seconds since last frame

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False

                elif event.key == pygame.K_SPACE:
                    paused = not paused

                elif event.key == pygame.K_r:
                    grid, age_grid = create_grid(randomize=True)
                    generation = 0

                elif event.key == pygame.K_c:
                    grid, age_grid = create_grid(randomize=False)
                    generation = 0

                elif event.key == pygame.K_t:
                    wrap_edges = not wrap_edges

                elif event.key == pygame.K_n:
                    if paused:
                        grid, age_grid = step(grid, age_grid, wrap_edges=wrap_edges)
                        generation += 1

                elif event.key == pygame.K_UP:
                    speed = min(60, speed + 1)

                elif event.key == pygame.K_DOWN:
                    speed = max(1, speed - 1)

                # Camera controls: I/J/K/L
                elif event.key == pygame.K_i:
                    camera_y = max(0, camera_y - 5)
                elif event.key == pygame.K_k:
                    camera_y = min(GRID_HEIGHT - 1, camera_y + 5)
                elif event.key == pygame.K_j:
                    camera_x = max(0, camera_x - 5)
                elif event.key == pygame.K_l:
                    camera_x = min(GRID_WIDTH - 1, camera_x + 5)

                # Zoom controls: Q/E
                elif event.key == pygame.K_q:
                    cell_size = max(MIN_CELL_SIZE, cell_size - 1)
                elif event.key == pygame.K_e:
                    cell_size = min(MAX_CELL_SIZE, cell_size + 1)

                # Patterns
                elif event.key == pygame.K_1:
                    mx, my = pygame.mouse.get_pos()
                    gx = camera_x + mx // cell_size
                    gy = camera_y + my // cell_size
                    stamp_pattern(grid, age_grid, gx, gy, GLIDER)

                elif event.key == pygame.K_2:
                    mx, my = pygame.mouse.get_pos()
                    gx = camera_x + mx // cell_size
                    gy = camera_y + my // cell_size
                    stamp_pattern(grid, age_grid, gx, gy, SMALL_EXPLODER)

                # Save / load
                elif event.key == pygame.K_F5:
                    save_pattern(grid)
                elif event.key == pygame.K_F9:
                    loaded = load_pattern()
                    if loaded is not None:
                        grid = loaded
                        # Reset ages to 1 for live cells, 0 for dead
                        age_grid = [
                            [1 if cell == 1 else 0 for cell in row]
                            for row in grid
                        ]
                        generation = 0

            # Mouse: toggle cells only when paused
            if event.type == pygame.MOUSEBUTTONDOWN and paused:
                mx, my = pygame.mouse.get_pos()
                gx = camera_x + mx // cell_size
                gy = camera_y + my // cell_size
                toggle_cell(grid, age_grid, gx, gy)

        # Simulation update
        if not paused:
            time_accumulator += dt
            step_interval = 1.0 / speed
            while time_accumulator >= step_interval:
                grid, age_grid = step(grid, age_grid, wrap_edges=wrap_edges)
                generation += 1
                time_accumulator -= step_interval

        draw_grid(screen, grid, age_grid, cell_size, camera_x, camera_y)
        draw_status(screen, font, generation, speed, paused, wrap_edges, cell_size, camera_x, camera_y)
        pygame.display.flip()

    pygame.quit()


if __name__ == "__main__":
    main()
