import pygame
import random
import sys
import math

# --- Config ---
CELL_SIZE = 15  # size of each grid cell in pixels
GRID_COLS = 100  # number of columns
GRID_ROWS = 60  # number of rows
WIDTH = GRID_COLS * CELL_SIZE
HEIGHT = GRID_ROWS * CELL_SIZE
FPS = 12  # base frames per second

# Colors (R, G, B)
BLACK = (0, 0, 0)
DARK_GREY = (40, 40, 40)
WHITE = (255, 255, 255)
FOOD_RED = (210, 60, 60)

# Worm colors (head to tail, blended)
WORM_HEAD_COLOR = (210, 160, 140)  # pinkish
WORM_TAIL_COLOR = (110, 60, 50)  # darker brownish


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


def lerp_color(c1, c2, t):
    """Linearly interpolate between two colors."""
    return (
        int(c1[0] + (c2[0] - c1[0]) * t),
        int(c1[1] + (c2[1] - c1[1]) * t),
        int(c1[2] + (c2[2] - c1[2]) * t),
    )


def draw_snake(surface, snake, frame_count, logical_dir):
    """
    Draw the snake as a smooth, wiggling worm:
      - circular segments
      - slight sine-wave wiggle
      - tapering radius
      - head with eyes
    """
    if not snake:
        return

    # Wiggle parameters
    wave_amplitude = CELL_SIZE * 0.20  # how far it wiggles sideways (pixels)
    wave_speed = 0.25  # how fast the wave moves
    wave_segment_spacing = 0.6  # phase offset per segment

    base_radius = CELL_SIZE * 0.45
    min_radius = CELL_SIZE * 0.25

    for i, (gx, gy) in enumerate(snake):
        # Determine direction of this segment (based on previous segment)
        if i == 0:
            dir_vec = logical_dir
        else:
            px, py = snake[i - 1]
            dx = gx - px
            dy = gy - py
            if dx == 0 and dy == 0:
                dir_vec = logical_dir
            else:
                dir_vec = (dx, dy)

        dx, dy = dir_vec

        # Perpendicular (normal) vector for lateral wiggle
        # Rotate (dx, dy) by +90 degrees -> (-dy, dx)
        nx, ny = -dy, dx

        # Normalizing not strictly necessary since dx,dy ∈ {-1,0,1},
        # but be safe in case of weird values.
        length = math.hypot(nx, ny)
        if length != 0:
            nx /= length
            ny /= length

        # Sine wave phase depends on time and segment index
        phase = frame_count * wave_speed + i * wave_segment_spacing
        offset_mag = wave_amplitude * math.sin(phase)

        # Grid center in pixels
        cx = gx * CELL_SIZE + CELL_SIZE / 2
        cy = gy * CELL_SIZE + CELL_SIZE / 2

        # Apply lateral offset
        cx += nx * offset_mag
        cy += ny * offset_mag

        # Taper radius towards tail
        if len(snake) > 1:
            t = i / (len(snake) - 1)
        else:
            t = 0.0
        radius = base_radius * (1 - 0.5 * t)
        radius = max(min_radius, radius)

        # Color gradient from head (t=0) to tail (t=1)
        color = lerp_color(WORM_HEAD_COLOR, WORM_TAIL_COLOR, t)

        pygame.draw.circle(surface, color, (int(cx), int(cy)), int(radius))

        # Draw eyes on the head only
        if i == 0:
            draw_worm_eyes(surface, cx, cy, radius, logical_dir)


def draw_worm_eyes(surface, cx, cy, radius, logical_dir):
    """Draw simple cartoon eyes at the front of the worm."""
    dx, dy = logical_dir
    # If not moving, default direction (right)
    if dx == 0 and dy == 0:
        dx, dy = 1, 0

    # Forward and sideways offsets
    forward = radius * 0.6
    side = radius * 0.6

    # Left-right perpendicular
    nx, ny = -dy, dx
    length = math.hypot(nx, ny)
    if length != 0:
        nx /= length
        ny /= length

    # Two eye centers
    eye1_x = cx + dx * forward + nx * side
    eye1_y = cy + dy * forward + ny * side
    eye2_x = cx + dx * forward - nx * side
    eye2_y = cy + dy * forward - ny * side

    eye_radius = max(2, int(radius * 0.25))
    pupil_radius = max(1, int(eye_radius * 0.4))

    # Eyes (white)
    pygame.draw.circle(surface, WHITE, (int(eye1_x), int(eye1_y)), eye_radius)
    pygame.draw.circle(surface, WHITE, (int(eye2_x), int(eye2_y)), eye_radius)

    # Pupils (black), slightly towards moving direction
    pupil_offset = radius * 0.2
    pupil1_x = eye1_x + dx * pupil_offset
    pupil1_y = eye1_y + dy * pupil_offset
    pupil2_x = eye2_x + dx * pupil_offset
    pupil2_y = eye2_y + dy * pupil_offset

    pygame.draw.circle(surface, BLACK, (int(pupil1_x), int(pupil1_y)), pupil_radius)
    pygame.draw.circle(surface, BLACK, (int(pupil2_x), int(pupil2_y)), pupil_radius)


def draw_food(surface, food_pos):
    x, y = food_pos
    rect = pygame.Rect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE)
    pygame.draw.rect(surface, FOOD_RED, rect, border_radius=int(CELL_SIZE * 0.3))


def draw_text(surface, text, size, color, center):
    font = pygame.font.SysFont("consolas", size, bold=True)
    surf = font.render(text, True, color)
    rect = surf.get_rect(center=center)
    surface.blit(surf, rect)


def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("Real Worm (Pygame Grid)")
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

    frame_count = 0

    while running:
        clock.tick(FPS)
        frame_count += 1

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
                    snake = [(GRID_COLS // 2, GRID_ROWS // 2)]
                    snake.append((snake[0][0] - 1, snake[0][1]))
                    snake.append((snake[0][0] - 2, snake[0][1]))
                    direction = (1, 0)
                    food = random_grid_position(snake)
                    score = 0
                    game_over = False

        if not game_over:
            # Move snake one cell per frame
            # Move snake one cell per frame
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

                # --- GROW WHEN EATING ---
                if new_head == food:
                    score += 1
                    food = random_grid_position(snake)
                    # Notice: NO snake.pop() here → length increases by 1
                else:
                    # No food eaten → remove tail → same length
                    snake.pop()

        # --- Drawing ---
        screen.fill(BLACK)
        draw_grid(screen)
        draw_snake(screen, snake, frame_count, direction)
        draw_food(screen, food)

        # Draw score at top-left
        draw_text(screen, f"Score: {score}", 20, WHITE, (80, 15))

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
