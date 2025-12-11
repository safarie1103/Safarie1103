import pygame
import random
import sys
import math

# --- Config ---
CELL_SIZE = 20  # size of each grid cell in pixels
GRID_COLS = 80  # number of columns
GRID_ROWS = 50  # number of rows
WIDTH = GRID_COLS * CELL_SIZE
HEIGHT = GRID_ROWS * CELL_SIZE
FPS = 12  # base frames per second

# Colors (R, G, B)
BLACK = (0, 0, 0)
DARK_GREY = (40, 40, 40)
WHITE = (255, 255, 255)
FOOD_RED = (210, 60, 60)


def random_grid_position(snake):
    """Return a random (x, y) position on the grid that is not on the snake."""
    while True:
        x = random.randrange(0, GRID_COLS)
        y = random.randrange(0, GRID_ROWS)
        if (x, y) not in snake:
            return x, y

def random_food_color():
    """Nice bright-ish random color for food."""
    return (
        random.randint(80, 255),
        random.randint(80, 255),
        random.randint(80, 255),
    )


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


def random_worm_colors():
    """Return a random pair of (head_color, tail_color)."""
    # Pick a random base color (not too dark)
    r = random.randint(80, 255)
    g = random.randint(80, 255)
    b = random.randint(80, 255)

    head = (r, g, b)
    # Tail is a darker version of the head
    tail = (int(r * 0.5), int(g * 0.5), int(b * 0.5))
    return head, tail


def draw_worm_eyes(surface, cx, cy, radius, logical_dir):
    """Draw simple cartoon eyes at the front of the worm."""
    dx, dy = logical_dir
    # If not moving, default direction (right)
    if dx == 0 and dy == 0:
        dx, dy = 1, 0

    # Forward and sideways offsets
    forward = radius * 0.75
    side = radius * 0.75

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

    eye_radius = max(5, int(radius * 0.25))
    pupil_radius = max(3, int(eye_radius * 0.4))

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


def draw_snake(surface, snake, frame_count, logical_dir, head_color, tail_color):
    """
    Draw the snake as a smooth, wiggling worm:
      - circular segments
      - slight sine-wave wiggle
      - tapering radius
      - head with eyes
      - gradient from head_color to tail_color
    """
    if not snake:
        return

    # Wiggle parameters
    wave_amplitude = CELL_SIZE * 0.20  # lateral wiggle in pixels
    wave_speed = 0.25  # how fast the wave moves
    wave_segment_spacing = 0.6  # phase offset per segment

    base_radius = CELL_SIZE * 0.45
    min_radius = CELL_SIZE * 0.25

    for i, (gx, gy) in enumerate(snake):
        # Direction for this segment
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

        # Perpendicular for wiggle
        nx, ny = -dy, dx
        length = math.hypot(nx, ny)
        if length != 0:
            nx /= length
            ny /= length

        # Sine-wave phase
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

        if i == 0:
            radius *= 1.6  # try 1.2–1.6 until it looks right
        # Gradient color from head → tail
        segment_color = lerp_color(head_color, tail_color, t)

        pygame.draw.circle(surface, segment_color, (int(cx), int(cy)), int(radius))

        # Head eyes
        if i == 0:
            draw_worm_eyes(surface, cx, cy, radius, logical_dir)


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
    # --- Audio init ---
    sound_move = sound_eat = sound_grow = sound_crash = None
    try:
        pygame.mixer.init()
        sound_move = pygame.mixer.Sound("move_loop.wav")
        sound_eat = pygame.mixer.Sound("eat.wav")
        sound_grow = pygame.mixer.Sound("grow.wav")
        sound_crash = pygame.mixer.Sound("crash.wav")

        sound_move.set_volume(0.4)
        sound_eat.set_volume(0.7)
        sound_grow.set_volume(0.7)
        sound_crash.set_volume(0.8)

    except Exception as e:
        # If audio fails (no device / files missing), game still works, just silent
        print("Audio init failed or sound files missing:", e)

    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("Real Worm (Fading Colors + Audio)")
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

    # --- Color palette fade state ---
    current_head_color, current_tail_color = random_worm_colors()
    palette_start_head = current_head_color
    palette_start_tail = current_tail_color
    palette_target_head = current_head_color
    palette_target_tail = current_tail_color
    palette_t = 1.0  # 0 → start, 1 → target (already at target)
    palette_fade_speed = 0.03  # how fast colors fade (per frame)

    # --- Continuous movement sound (loop) ---
    move_channel = None
    if sound_move is not None:
        move_channel = sound_move.play(loops=-1)

    def reset_game_state():
        nonlocal snake, direction, food, score, game_over
        nonlocal current_head_color, current_tail_color
        nonlocal palette_start_head, palette_start_tail
        nonlocal palette_target_head, palette_target_tail, palette_t
        snake = [(GRID_COLS // 2, GRID_ROWS // 2)]
        snake.append((snake[0][0] - 1, snake[0][1]))
        snake.append((snake[0][0] - 2, snake[0][1]))
        direction = (1, 0)
        food = random_grid_position(snake)
        score = 0
        game_over = False

        current_head_color, current_tail_color = random_worm_colors()
        palette_start_head = current_head_color
        palette_start_tail = current_tail_color
        palette_target_head = current_head_color
        palette_target_tail = current_tail_color
        palette_t = 1.0

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
                    reset_game_state()
                    # restart movement loop if audio available
                    if sound_move is not None:
                        sound_move.stop()
                        sound_move.play(loops=-1)

        if not game_over:
            # Move snake one cell per frame
            head_x, head_y = snake[0]
            dx, dy = direction
            new_head = (head_x + dx, head_y + dy)
            
            # Check wall collision
            if (new_head[0] < 0 or new_head[0] >= GRID_COLS or new_head[1] < 0 or new_head[1] >= GRID_ROWS):
                game_over = True
                sound_move.stop()
                # 🔊 stop move loop + play crash
                if sound_move is not None and move_channel is not None:
                    move_channel.fadeout(300)  # or .stop() for instant cut
                    move_channel = None
                if sound_crash is not None:
                    sound_crash.play()

            # Check self-collision
            elif new_head in snake:
                game_over = True
                sound_move.stop()
                if sound_move is not None and move_channel is not None:
                    move_channel.fadeout(300)
                    move_channel = None
                if sound_crash is not None:
                    sound_crash.play()
            else:
                # Add new head
                snake.insert(0, new_head)

                # Grow when eating: don't pop tail this frame
                if new_head == food:
                    score += 1
                    food = random_grid_position(snake)

                    # Play eating and growing sounds
                    if sound_eat is not None:
                        sound_eat.play()
                    if sound_grow is not None:
                        sound_grow.play()

                    # Start a new palette fade
                    current_head_color = lerp_color(
                        palette_start_head, palette_target_head, min(palette_t, 1.0)
                    )
                    current_tail_color = lerp_color(
                        palette_start_tail, palette_target_tail, min(palette_t, 1.0)
                    )

                    palette_start_head = current_head_color
                    palette_start_tail = current_tail_color
                    palette_target_head, palette_target_tail = random_worm_colors()
                    palette_t = 0.0

                else:
                    # No food: remove tail -> same length
                    snake.pop()


        # --- Update palette fade each frame ---
        if palette_t < 1.0:
            palette_t += palette_fade_speed
            if palette_t > 1.0:
                palette_t = 1.0

        # Effective colors for this frame
        effective_head_color = lerp_color(
            palette_start_head, palette_target_head, palette_t
        )
        effective_tail_color = lerp_color(
            palette_start_tail, palette_target_tail, palette_t
        )

        # --- Drawing ---
        screen.fill(BLACK)
        draw_grid(screen)
        draw_snake(
            screen,
            snake,
            frame_count,
            direction,
            effective_head_color,
            effective_tail_color,
        )
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
