import pygame
import random

# ----- CONFIG -----
CELL_SIZE = 10          # pixels per cell
GRID_WIDTH = 80         # number of cells horizontally
GRID_HEIGHT = 60        # number of cells vertically
FPS = 15                # simulation speed (frames per second)
ALIVE_COLOR = (0, 255, 0)
DEAD_COLOR = (0, 0, 0)
GRID_COLOR = (40, 40, 40)
# -------------------


def create_grid(randomize=False):
    """Create a GRID_HEIGHT x GRID_WIDTH grid."""
    return [
        [1 if randomize and random.random() < 0.2 else 0 for _ in range(GRID_WIDTH)]
        for _ in range(GRID_HEIGHT)
    ]


def count_neighbors(grid, x, y):
    """Count live neighbors around cell (x, y)."""
    neighbors = 0
    for dy in (-1, 0, 1):
        for dx in (-1, 0, 1):
            if dx == 0 and dy == 0:
                continue
            nx, ny = x + dx, y + dy
            if 0 <= nx < GRID_WIDTH and 0 <= ny < GRID_HEIGHT:
                neighbors += grid[ny][nx]
    return neighbors


def step(grid):
    """Compute the next generation."""
    new_grid = [[0 for _ in range(GRID_WIDTH)] for _ in range(GRID_HEIGHT)]
    for y in range(GRID_HEIGHT):
        for x in range(GRID_WIDTH):
            alive = grid[y][x] == 1
            neighbors = count_neighbors(grid, x, y)

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
                rect = pygame.Rect(
                    x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE
                )
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

    pygame.display.flip()


def main():
    pygame.init()
    screen = pygame.display.set_mode(
        (GRID_WIDTH * CELL_SIZE, GRID_HEIGHT * CELL_SIZE)
    )
    pygame.display.set_caption("Conway's Game of Life")

    clock = pygame.time.Clock()

    grid = create_grid(randomize=True)
    running = True
    paused = False

    while running:
        clock.tick(FPS)

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            if event.type == pygame.KSCAN_F1:
                print("F1 Key Pressed")
                grid = create_grid(randomize=True)
            # Mouse: toggle cells when paused
            if event.type == pygame.MOUSEBUTTONDOWN:
                x, y = pygame.mouse.get_pos()
                grid_x = x // CELL_SIZE
                grid_y = y // CELL_SIZE
                if 0 <= grid_x < GRID_WIDTH and 0 <= grid_y < GRID_HEIGHT:
                    grid[grid_y][grid_x] = 1 - grid[grid_y][grid_x]
                    print("Toggled cell:", grid_x, grid_y)

            # Keyboard controls
            
            if event.type == pygame.KEYDOWN:
                print("KEYDOWN Pressed")
                if event.key == pygame.K_SPACE:
                    print("K_SPACE Pressed")
                    paused = not paused
                elif event.key == pygame.K_r:
                    print("K_r Pressed")
                    grid = create_grid(randomize=True)
                elif event.key == pygame.K_c:
                    print("K_c Pressed")
                    grid = create_grid(randomize=False)
        
        if not paused:
            grid = step(grid)

        draw_grid(screen, grid)

    pygame.quit()


if __name__ == "__main__":
    main()
