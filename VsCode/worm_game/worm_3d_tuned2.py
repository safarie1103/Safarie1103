
import pygame
import random
import sys
import math

# --- Config ---
GRID_SIZE = 14          # 3D grid: 0..GRID_SIZE-1 for x, y, z
FPS = 10

# Big window
WIDTH, HEIGHT = 1600, 900

# Projection params:
# - Larger BASE_SCALE and smaller DEPTH_OFFSET make the cube look bigger.
# - Segment radius is clamped so the worm doesn't get huge.
DEPTH_OFFSET = 2.5
BASE_SCALE = 350.0

# Colors
BLACK = (0, 0, 0)
BG = (10, 10, 20)
WHITE = (255, 255, 255)
GRID_COLOR = (40, 40, 60)
FOOD_COLOR = (255, 120, 80)
HEAD_COLOR = (120, 220, 255)
TAIL_COLOR = (40, 100, 160)
GAME_OVER_COLOR = (255, 80, 80)


def project_point(x, y, z):
    """Simple perspective projection from 3D (x,y,z) to 2D screen coords."""
    cx = GRID_SIZE / 2
    cy = GRID_SIZE / 2
    cz = GRID_SIZE / 2

    # relative to center
    rx = x - cx
    ry = y - cy
    rz = z - cz

    # depth factor
    depth = rz + DEPTH_OFFSET + GRID_SIZE / 2
    if depth < 0.1:
        depth = 0.1

    scale = BASE_SCALE / depth

    screen_x = WIDTH / 2 + rx * scale
    screen_y = HEIGHT / 2 + ry * scale

    return int(screen_x), int(screen_y), scale


def draw_text(surface, text, size, color, center):
    font = pygame.font.SysFont("consolas", size, bold=True)
    surf = font.render(text, True, color)
    rect = surf.get_rect(center=center)
    surface.blit(surf, rect)


def random_free_position(snake, food):
    """Return a random (x,y,z) not occupied by snake or food."""
    occupied = set(snake)
    if food is not None:
        occupied.add(food)
    while True:
        pos = (
            random.randrange(0, GRID_SIZE),
            random.randrange(0, GRID_SIZE),
            random.randrange(0, GRID_SIZE),
        )
        if pos not in occupied:
            return pos


def draw_grid_3d(surface):
    """Draw a wireframe cube (front, middle, back slices)."""
    for z in (0, GRID_SIZE // 2, GRID_SIZE - 1):
        pts = []
        corners = [
            (0, 0, z),
            (GRID_SIZE - 1, 0, z),
            (GRID_SIZE - 1, GRID_SIZE - 1, z),
            (0, GRID_SIZE - 1, z),
        ]
        for (x, y, zc) in corners:
            sx, sy, _ = project_point(x, y, zc)
            pts.append((sx, sy))
        pygame.draw.lines(surface, GRID_COLOR, True, pts, 1)


def draw_snake_3d(surface, snake):
    """Draw the snake in 3D with depth sorting and gradient."""
    if not snake:
        return

    segs = []
    for i, (x, y, z) in enumerate(snake):
        _, _, scale = project_point(x, y, z)
        segs.append((z, i, x, y, scale))

    # sort by z: farthest first
    segs.sort(key=lambda t: t[0], reverse=True)

    length = len(snake)

    for z, i, x, y, scale in segs:
        if length > 1:
            t = i / (length - 1)
        else:
            t = 0.0
        r = int(HEAD_COLOR[0] + (TAIL_COLOR[0] - HEAD_COLOR[0]) * t)
        g = int(HEAD_COLOR[1] + (TAIL_COLOR[1] - HEAD_COLOR[1]) * t)
        b = int(HEAD_COLOR[2] + (TAIL_COLOR[2] - HEAD_COLOR[2]) * t)
        color = (r, g, b)

        sx, sy, scale = project_point(x, y, z)
        # Smaller, clamped radius so worm doesn't get huge
        radius = int(1.5 * scale)
        radius = max(4, min(18, radius))

        if i == 0:
            radius = int(radius * 1.2)
            color = HEAD_COLOR

        pygame.draw.circle(surface, color, (sx, sy), radius)

        # Head eyes
        if i == 0:
            eye_r = max(2, int(radius * 0.3))
            pygame.draw.circle(surface, WHITE, (sx - eye_r, sy - eye_r), eye_r)
            pygame.draw.circle(surface, WHITE, (sx + eye_r, sy - eye_r), eye_r)


def draw_food_3d(surface, food):
    if food is None:
        return
    x, y, z = food
    sx, sy, scale = project_point(x, y, z)
    radius = int(1.2 * scale)
    radius = max(3, min(14, radius))
    pygame.draw.circle(surface, FOOD_COLOR, (sx, sy), radius)


def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("3D Worm (bigger grid, tuned sizes)")
    clock = pygame.time.Clock()

    cy = GRID_SIZE // 2
    cz = GRID_SIZE // 2
    snake = [
        (GRID_SIZE // 2, cy, cz),
        (GRID_SIZE // 2 - 1, cy, cz),
        (GRID_SIZE // 2 - 2, cy, cz),
    ]
    direction = (1, 0, 0)

    food = random_free_position(snake, None)

    score = 0
    game_over = False
    running = True

    while running:
        clock.tick(FPS)

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    running = False

                if not game_over:
                    # Movement in 3D
                    if event.key == pygame.K_LEFT:
                        if direction != (1, 0, 0):
                            direction = (-1, 0, 0)
                    elif event.key == pygame.K_RIGHT:
                        if direction != (-1, 0, 0):
                            direction = (1, 0, 0)
                    elif event.key == pygame.K_UP:
                        if direction != (0, 1, 0):
                            direction = (0, -1, 0)
                    elif event.key == pygame.K_DOWN:
                        if direction != (0, -1, 0):
                            direction = (0, 1, 0)
                    elif event.key == pygame.K_q:
                        if direction != (0, 0, 1):
                            direction = (0, 0, -1)
                    elif event.key == pygame.K_e:
                        if direction != (0, 0, -1):
                            direction = (0, 0, 1)
                else:
                    if event.key == pygame.K_r:
                        score = 0
                        game_over = False
                        snake = [
                            (GRID_SIZE // 2, cy, cz),
                            (GRID_SIZE // 2 - 1, cy, cz),
                            (GRID_SIZE // 2 - 2, cy, cz),
                        ]
                        direction = (1, 0, 0)
                        food = random_free_position(snake, None)

        if not game_over:
            head_x, head_y, head_z = snake[0]
            dx, dy, dz = direction
            new_head = (head_x + dx, head_y + dy, head_z + dz)

            x, y, z = new_head
            if not (0 <= x < GRID_SIZE and 0 <= y < GRID_SIZE and 0 <= z < GRID_SIZE):
                game_over = True
            elif new_head in snake:
                game_over = True
            else:
                snake.insert(0, new_head)

                if new_head == food:
                    score += 1
                    food = random_free_position(snake, None)
                else:
                    snake.pop()

        screen.fill(BG)
        draw_grid_3d(screen)
        draw_food_3d(screen, food)
        draw_snake_3d(screen, snake)

        draw_text(screen, f"Score: {score}", 28, WHITE, (130, 40))
        draw_text(
            screen,
            "Arrows: X/Y  |  Q/E: Z  |  R: restart  |  Esc: quit",
            24,
            WHITE,
            (WIDTH // 2, HEIGHT - 40),
        )

        if game_over:
            draw_text(
                screen,
                "GAME OVER",
                54,
                GAME_OVER_COLOR,
                (WIDTH // 2, HEIGHT // 2 - 50),
            )
            draw_text(
                screen,
                "Press R to restart  |  Esc to quit",
                28,
                WHITE,
                (WIDTH // 2, HEIGHT // 2 + 10),
            )

        pygame.display.flip()

    pygame.quit()
    sys.exit()


if __name__ == "__main__":
    main()
