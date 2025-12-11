import pygame
import random

# ----- CONFIG -----
CELL_SIZE = 10  # pixels per cell
GRID_WIDTH = 80  # number of cells horizontally
GRID_HEIGHT = 60  # number of cells vertically
INITIAL_SPEED = 10  # generations per second (when running)
ALIVE_COLOR = (0, 255, 0)
DEAD_COLOR = (0, 0, 0)
GRID_COLOR = (40, 40, 40)
TEXT_COLOR = (200, 200, 200)
BG_TEXT_COLOR = (0, 0, 0)
# -------------------


def create_grid(randomize=False):
    """Create a GRID_HEIGHT x GRID_WIDTH grid."""
    return [
        [1 if randomize and random.random() < 0.2 else 0 for _ in range(GRID_WIDTH)]
        for _ in range(GRID_HEIGHT)
    ]


def count_neighbors(grid, x, y, wrap_edges=False):
    """Count live neighbors around cell (x, y)."""
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


def step(grid, wrap_edges=False):
    """Compute the next generation."""
    new_grid = [[0 for _ in range(GRID_WIDTH)] for _ in range(GRID_HEIGHT)]
    for y in range(GRID_HEIGHT):
        for x in range(GRID_WIDTH):
            alive = grid[y][x] == 1
            neighbors = count_neighbors(grid, x, y, wrap_edges=wrap_edges)

            if alive:
                # Live cell survives with 2 or 3 neighbors
                if neighbors == 2 or neighbors == 3:
                    new_grid[y][x] = 1
                else:
                    new_grid[y][x] = 0
            else:
                # Dead cell becomes alive if exactly 3 neighbors
                if neighbors == 3:
                    new_grid[y][x] = 1
                else:
                    new_grid[y][x] = 0
    return new_grid


def draw_grid(screen, grid):
    """Draw the grid and the cells."""
    screen.fill(DEAD_COLOR)

    # Draw live cells
    for y in range(GRID_HEIGHT):
        for x in range(GRID_WIDTH):
            if grid[y][x] == 1:
                rect = pygame.Rect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE)
                pygame.draw.rect(screen, ALIVE_COLOR, rect)

    # Draw grid lines
    for x in range(GRID_WIDTH + 1):
        pygame.draw.line(
            screen,
            GRID_COLOR,
            (x * CELL_SIZE, 0),
            (x * CELL_SIZE, GRID_HEIGHT * CELL_SIZE),
            1,
        )
    for y in range(GRID_HEIGHT + 1):
        pygame.draw.line(
            screen,
            GRID_COLOR,
            (0, y * CELL_SIZE),
            (GRID_WIDTH * CELL_SIZE, y * CELL_SIZE),
            1,
        )


def draw_status(screen, font, generation, speed, paused, wrap_edges):
    """Draw status text (generation, speed, controls)."""
    lines = [
        f"Gen: {generation}   Speed: {speed} gen/s   "
        f"{'PAUSED' if paused else 'RUNNING'}   "
        f"Wrap: {'ON' if wrap_edges else 'OFF'}",
        "SPACE: pause/run   S: step   R: random   C: clear",
        "W: toggle wrap     UP/DOWN: speed +/-   1: glider   2: small exploder",
        "Click (when paused) to toggle cells.",
    ]

    # Draw text with a dark background strip at the top
    padding = 4
    line_height = font.get_linesize()
    box_height = line_height * len(lines) + padding * 2
    box_rect = pygame.Rect(0, 0, GRID_WIDTH * CELL_SIZE, box_height)
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
    (0, 2),
    (1, 2),
    (2, 2),
]

SMALL_EXPLODER = [
    (0, 1),
    (-1, 0),
    (0, 0),
    (1, 0),
    (-1, -1),
    (1, -1),
    (0, -2),
]


def stamp_pattern(grid, x, y, pattern):
    """Stamp a pattern (list of (dx, dy)) with origin at (x, y)."""
    for dx, dy in pattern:
        gx = x + dx
        gy = y + dy
        if 0 <= gx < GRID_WIDTH and 0 <= gy < GRID_HEIGHT:
            grid[gy][gx] = 1


def main():
    pygame.init()
    screen = pygame.display.set_mode((GRID_WIDTH * CELL_SIZE, GRID_HEIGHT * CELL_SIZE))
    pygame.display.set_caption("Conway's Game of Life - Advanced")

    clock = pygame.time.Clock()
    font = pygame.font.SysFont("consolas", 16)

    grid = create_grid(randomize=False)
    running = True
    paused = True  # start paused so you can draw
    wrap_edges = False
    generation = 0

    speed = INITIAL_SPEED  # generations per second, 1..60

    # For timing so speed is independent of display FPS
    time_accumulator = 0.0

    while running:
        dt = clock.tick(60) / 1000.0  # seconds since last frame (target 60 FPS)

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            # Mouse: toggle cells only when paused
            if event.type == pygame.MOUSEBUTTONDOWN and paused:
                x, y = pygame.mouse.get_pos()
                grid_x = x // CELL_SIZE
                grid_y = y // CELL_SIZE
                if 0 <= grid_x < GRID_WIDTH and 0 <= grid_y < GRID_HEIGHT:
                    grid[grid_y][grid_x] = 1 - grid[grid_y][grid_x]

            # Keyboard controls
            if event.type == pygame.KEYDOWN:
                if event.key == pygame.K_SPACE:
                    paused = not paused

                elif event.key == pygame.K_r:
                    grid = create_grid(randomize=True)
                    generation = 0

                elif event.key == pygame.K_c:
                    grid = create_grid(randomize=False)
                    generation = 0

                elif event.key == pygame.K_w:
                    wrap_edges = not wrap_edges

                elif event.key == pygame.K_s:
                    # Step one generation when paused
                    if paused:
                        grid = step(grid, wrap_edges=wrap_edges)
                        generation += 1

                elif event.key == pygame.K_UP:
                    speed = min(60, speed + 1)

                elif event.key == pygame.K_DOWN:
                    speed = max(1, speed - 1)

                # Patterns
                elif event.key == pygame.K_1:
                    # Stamp glider at mouse position
                    x, y = pygame.mouse.get_pos()
                    grid_x = x // CELL_SIZE
                    grid_y = y // CELL_SIZE
                    stamp_pattern(grid, grid_x, grid_y, GLIDER)

                elif event.key == pygame.K_2:
                    # Stamp small exploder at mouse position
                    x, y = pygame.mouse.get_pos()
                    grid_x = x // CELL_SIZE
                    grid_y = y // CELL_SIZE
                    stamp_pattern(grid, grid_x, grid_y, SMALL_EXPLODER)

        # Simulation update
        if not paused:
            time_accumulator += dt
            step_interval = 1.0 / speed  # seconds per generation
            # Handle multiple steps if we're behind
            while time_accumulator >= step_interval:
                grid = step(grid, wrap_edges=wrap_edges)
                generation += 1
                time_accumulator -= step_interval

        # Drawing
        draw_grid(screen, grid)
        draw_status(screen, font, generation, speed, paused, wrap_edges)
        pygame.display.flip()

    pygame.quit()


if __name__ == "__main__":
    main()
