import pygame
import random
import sys

# --- Config ---
CELL_SIZE = 15  # size of each grid cell in pixels
GRID_COLS = 100  # number of columns
GRID_ROWS = 60  # number of rows
WIDTH = GRID_COLS * CELL_SIZE
HEIGHT = GRID_ROWS * CELL_SIZE
FPS = 10  # snake speed (frames per second)

# Colors (R, G, B)
BLACK = (0, 0, 0)
DARK_GREY = (40, 40, 40)
GREEN = (0, 200, 0)
RED = (200, 0, 0)
WHITE = (255, 255, 255)


def random_grid_position(snake):
    """Return a random (x, y) position on the grid that is not on the snake."""
    while True:
        x = random.randrange(0, GRID_COLS)
        y = random.randrange(0, GRID_ROWS)
        if (x, y) not in snake:
            return x, y


def draw_grid(surface):
    """Draw the background grid lines."""
    for x in range(0, WIDTH, CELL_SIZE):
        pygame.draw.line(surface, DARK_GREY, (x, 0), (x, HEIGHT))
    for y in range(0, HEIGHT, CELL_SIZE):
        pygame.draw.line(surface, DARK_GREY, (0, y), (WIDTH, y))


def draw_snake(surface, snake):
    """Draw the snake (list of (x, y) grid positions)."""
    for x, y in snake:
        rect = pygame.Rect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE)
        pygame.draw.rect(surface, GREEN, rect)


def draw_food(surface, food_pos):
    x, y = food_pos
    rect = pygame.Rect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE)
    pygame.draw.rect(surface, RED, rect)


def draw_text(surface, text, size, color, center):
    font = pygame.font.SysFont("consolas", size, bold=True)
    surf = font.render(text, True, color)
    rect = surf.get_rect(center=center)
    surface.blit(surf, rect)


def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("Worm Game (Pygame Grid)")
    clock = pygame.time.Clock()

    # Snake state: list of (x, y) on grid; head is snake[0]
    snake = [(GRID_COLS // 2, GRID_ROWS // 2)]
    snake.append((snake[0][0] - 1, snake[0][1]))
    snake.append((snake[0][0] - 2, snake[0][1]))

    direction = (1, 0)  # moving right initially (dx, dy)

    food = random_grid_position(snake)
    score = 0
    running = True
    game_over = False

    while running:
        clock.tick(FPS)

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            elif event.type == pygame.KEYDOWN:
                # Movement controls (arrow keys or WASD)
                if event.key in (pygame.K_RIGHT, pygame.K_d):
                    if direction != (-1, 0):
                        direction = (1, 0)
                elif event.key in (pygame.K_LEFT, pygame.K_a):
                    if direction != (1, 0):
                        direction = (-1, 0)
                elif event.key in (pygame.K_UP, pygame.K_w):
                    if direction != (0, 1):
                        direction = (0, -1)
                elif event.key in (pygame.K_DOWN, pygame.K_s):
                    if direction != (0, -1):
                        direction = (0, 1)

                # Restart when game over
                if game_over and event.key == pygame.K_r:
                    # Reset game state
                    snake = [(GRID_COLS // 2, GRID_ROWS // 2)]
                    snake.append((snake[0][0] - 1, snake[0][1]))
                    snake.append((snake[0][0] - 2, snake[0][1]))
                    direction = (1, 0)
                    food = random_grid_position(snake)
                    score = 0
                    game_over = False

        if not game_over:
            # Move snake
            head_x, head_y = snake[0]
            dx, dy = direction
            new_head = (head_x + dx, head_y + dy)

            # Check wall collision
            if (
                new_head[0] < 0
                or new_head[0] >= GRID_COLS
                or new_head[1] < 0
                or new_head[1] >= GRID_ROWS
            ):
                game_over = True
            # Check self-collision
            elif new_head in snake:
                game_over = True
            else:
                # Add new head
                snake.insert(0, new_head)

                # Check food collision
                if new_head == food:
                    score += 1
                    food = random_grid_position(snake)
                else:
                    # Remove tail
                    snake.pop()

        # --- Drawing ---
        screen.fill(BLACK)
        draw_grid(screen)
        draw_snake(screen, snake)
        draw_food(screen, food)

        # Draw score in top-left
        draw_text(screen, f"Score: {score}", 20, WHITE, (70, 15))

        if game_over:
            draw_text(screen, "GAME OVER", 40, WHITE, (WIDTH // 2, HEIGHT // 2 - 20))
            draw_text(
                screen,
                "Press R to restart or close window",
                20,
                WHITE,
                (WIDTH // 2, HEIGHT // 2 + 20),
            )

        pygame.display.flip()

    pygame.quit()
    sys.exit()


if __name__ == "__main__":
    main()
