import pygame
import random
import sys
import math
import json
import os

# --- Config ---
CELL_SIZE = 20  # size of each grid cell in pixels
GRID_COLS = 60  # number of columns
GRID_ROWS = 40  # number of rows
WIDTH = GRID_COLS * CELL_SIZE
HEIGHT = GRID_ROWS * CELL_SIZE
FPS = 12  # base frames per second
NUM_FOODS = 3  # how many foods on screen at once
FOOD_LIFETIME = 30.0  # seconds before a food expires
FOOD_ALARM_TIME = 10.0  # seconds before expiry to play alarm

SCORES_FILE = "worm_scores.json"

# Colors (R, G, B)
BLACK = (0, 0, 0)
DARK_GREY = (40, 40, 40)
WHITE = (255, 255, 255)
RED = (255, 0, 0)


def random_grid_position(blocked):
    """Return a random (x, y) position not in the blocked set."""
    while True:
        x = random.randrange(0, GRID_COLS)
        y = random.randrange(0, GRID_ROWS)
        if (x, y) not in blocked:
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


def random_worm_colors():
    """Return a random pair of (head_color, tail_color)."""
    r = random.randint(80, 255)
    g = random.randint(80, 255)
    b = random.randint(80, 255)

    head = (r, g, b)
    tail = (int(r * 0.5), int(g * 0.5), int(b * 0.5))
    return head, tail


def random_food_color():
    """Nice bright-ish random color for food."""
    return (
        random.randint(80, 255),
        random.randint(80, 255),
        random.randint(80, 255),
    )


def make_food(blocked):
    """Create a new food dict with position, color, and timer."""
    pos = random_grid_position(blocked)
    color = random_food_color()
    time_left = int(FOOD_LIFETIME * FPS)  # frames remaining
    return {
        "pos": pos,
        "color": color,
        "time_left": time_left,
        "alarm_played": False,
    }


def draw_worm_eyes(surface, cx, cy, radius, logical_dir):
    """Draw simple cartoon eyes at the front of the worm."""
    dx, dy = logical_dir
    if dx == 0 and dy == 0:
        dx, dy = 1, 0

    # Offsets
    forward = radius * 0.75
    side = radius * 0.75

    nx, ny = -dy, dx
    length = math.hypot(nx, ny)
    if length != 0:
        nx /= length
        ny /= length

    eye1_x = cx + dx * forward + nx * side
    eye1_y = cy + dy * forward + ny * side
    eye2_x = cx + dx * forward - nx * side
    eye2_y = cy + dy * forward - ny * side

    eye_radius = max(3, int(radius * 0.4))
    pupil_radius = max(2, int(eye_radius * 0.55))

    pygame.draw.circle(surface, WHITE, (int(eye1_x), int(eye1_y)), eye_radius)
    pygame.draw.circle(surface, WHITE, (int(eye2_x), int(eye2_y)), eye_radius)

    pupil_offset = radius * 0.2
    pupil1_x = eye1_x + dx * pupil_offset
    pupil1_y = eye1_y + dy * pupil_offset
    pupil2_x = eye2_x + dx * pupil_offset
    pupil2_y = eye2_y + dy * pupil_offset

    pygame.draw.circle(surface, BLACK, (int(pupil1_x), int(pupil1_y)), pupil_radius)
    pygame.draw.circle(surface, BLACK, (int(pupil2_x), int(pupil2_y)), pupil_radius)


def draw_snake(surface, snake, frame_count, logical_dir, head_color, tail_color):
    """Draw the snake as a smooth, wiggling worm."""
    if not snake:
        return

    wave_amplitude = CELL_SIZE * 0.20
    wave_speed = 0.25
    wave_segment_spacing = 0.6

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

        phase = frame_count * wave_speed + i * wave_segment_spacing
        offset_mag = wave_amplitude * math.sin(phase)

        cx = gx * CELL_SIZE + CELL_SIZE / 2
        cy = gy * CELL_SIZE + CELL_SIZE / 2

        cx += nx * offset_mag
        cy += ny * offset_mag

        # Taper radius
        if len(snake) > 1:
            t = i / (len(snake) - 1)
        else:
            t = 0.0
        radius = base_radius * (1 - 0.5 * t)
        radius = max(min_radius, radius)

        # Make head bigger
        if i == 0:
            radius *= 1.4

        segment_color = lerp_color(head_color, tail_color, t)
        pygame.draw.circle(surface, segment_color, (int(cx), int(cy)), int(radius))

        if i == 0:
            draw_worm_eyes(surface, cx, cy, radius, logical_dir)


def draw_foods(surface, foods):
    for food in foods:
        x, y = food["pos"]
        color = food["color"]
        rect = pygame.Rect(x * CELL_SIZE, y * CELL_SIZE, CELL_SIZE, CELL_SIZE)
        pygame.draw.rect(surface, color, rect, border_radius=int(CELL_SIZE * 0.3))


def draw_text(surface, text, size, color, center):
    font = pygame.font.SysFont("consolas", size, bold=True)
    surf = font.render(text, True, color)
    rect = surf.get_rect(center=center)
    surface.blit(surf, rect)


def load_scores(path=SCORES_FILE):
    if not os.path.exists(path):
        return {}
    try:
        with open(path, "r", encoding="utf-8") as f:
            return json.load(f)
    except Exception:
        return {}


def save_scores(scores, path=SCORES_FILE):
    try:
        with open(path, "w", encoding="utf-8") as f:
            json.dump(scores, f, indent=2)
    except Exception:
        pass


def update_user_scores(scores, username, score):
    data = scores.get(username)
    if data is None:
        data = {"scores": [], "best": score}
    data["scores"].append(score)
    if score > data.get("best", 0):
        data["best"] = score
    scores[username] = data
    return data


def get_username_pygame():
    """Ask for username inside the game window using Pygame."""
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("Real Worm – Enter Your Name")
    clock = pygame.time.Clock()

    font = pygame.font.SysFont("consolas", 32, bold=True)
    small_font = pygame.font.SysFont("consolas", 20)

    username = ""
    active = True

    while active:
        clock.tick(30)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return "Player"
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_RETURN:
                    return username.strip() or "Player"
                elif event.key == pygame.K_BACKSPACE:
                    username = username[:-1]
                elif event.key == pygame.K_ESCAPE:
                    return "Player"
                else:
                    if event.unicode and event.unicode.isprintable():
                        if len(username) < 16:
                            username += event.unicode

        screen.fill(BLACK)
        prompt_surf = font.render("Enter your name:", True, WHITE)
        prompt_rect = prompt_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 40))
        screen.blit(prompt_surf, prompt_rect)

        display_name = username if username else "_"
        name_surf = font.render(display_name, True, WHITE)
        name_rect = name_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 5))
        screen.blit(name_surf, name_rect)

        info_surf = small_font.render(
            "Press Enter to start  |  Esc to skip", True, WHITE
        )
        info_rect = info_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 50))
        screen.blit(info_surf, info_rect)

        pygame.display.flip()


def run_game(username):
    pygame.init()

    # --- Audio init ---
    sound_move = sound_eat = sound_grow = sound_crash = None
    sound_pop = sound_spawn = None
    sound_alarm = None

    try:
        pygame.mixer.init()
        sound_move = pygame.mixer.Sound("move_loop.wav")
        sound_eat = pygame.mixer.Sound("eat.wav")
        sound_grow = pygame.mixer.Sound("grow.wav")
        sound_crash = pygame.mixer.Sound("crash.wav")
        sound_pop = pygame.mixer.Sound("pop.wav")  # when food expires
        sound_spawn = pygame.mixer.Sound("spawn.wav")  # when new food appears
        sound_alarm = pygame.mixer.Sound("alarm.wav")  # when food nearly expires

        sound_move.set_volume(0.4)
        sound_eat.set_volume(0.7)
        sound_grow.set_volume(0.7)
        if sound_crash:
            sound_crash.set_volume(0.8)
        if sound_pop:
            sound_pop.set_volume(0.7)
        if sound_spawn:
            sound_spawn.set_volume(0.7)
        if sound_alarm:
            sound_alarm.set_volume(0.7)
    except Exception as e:
        print("Audio init failed or sound files missing:", e)

    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption(f"Real Worm – Timed Foods ({username})")
    clock = pygame.time.Clock()

    # Load score history
    scores = load_scores()
    user_data = scores.get(username, {"scores": [], "best": 0})
    user_best = user_data.get("best", 0)

    # --- Game state ---

    snake = [
        (GRID_COLS // 2, GRID_ROWS // 2),
        (GRID_COLS // 2 - 1, GRID_ROWS // 2),
        (GRID_COLS // 2 - 2, GRID_ROWS // 2),
    ]
    direction = (1, 0)

    # Foods
    foods = []
    blocked = set(snake)
    for _ in range(NUM_FOODS):
        food = make_food(blocked)
        foods.append(food)
        blocked.add(food["pos"])

    score = 0
    running = True
    game_over = False
    frame_count = 0

    # Color palette fade state
    current_head_color, current_tail_color = random_worm_colors()
    palette_start_head = current_head_color
    palette_start_tail = current_tail_color
    palette_target_head = current_head_color
    palette_target_tail = current_tail_color
    palette_t = 1.0
    palette_fade_speed = 0.03

    # --- Countdown before game starts ---
    def show_countdown():
        countdown_font_big = pygame.font.SysFont("consolas", 48, bold=True)
        countdown_font_small = pygame.font.SysFont("consolas", 28)

        for n in range(3, 0, -1):
            start_ticks = pygame.time.get_ticks()
            while pygame.time.get_ticks() - start_ticks < 1000:
                for event in pygame.event.get():
                    if event.type == pygame.QUIT:
                        pygame.quit()
                        sys.exit()

                screen.fill(BLACK)
                msg = f"Get ready, {username}!"
                msg_surf = countdown_font_small.render(msg, True, WHITE)
                msg_rect = msg_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 40))
                screen.blit(msg_surf, msg_rect)

                num_surf = countdown_font_big.render(str(n), True, WHITE)
                num_rect = num_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 20))
                screen.blit(num_surf, num_rect)

                pygame.display.flip()
                clock.tick(60)

    show_countdown()

    # Looping movement sound starts AFTER countdown
    if sound_move is not None:
        sound_move.play(loops=-1)

    def reset_game_state():
        nonlocal snake, direction, foods, score, game_over
        nonlocal current_head_color, current_tail_color
        nonlocal palette_start_head, palette_start_tail
        nonlocal palette_target_head, palette_target_tail, palette_t

        snake = [
            (GRID_COLS // 2, GRID_ROWS // 2),
            (GRID_COLS // 2 - 1, GRID_ROWS // 2),
            (GRID_COLS // 2 - 2, GRID_ROWS // 2),
        ]
        direction = (1, 0)

        foods = []
        blocked_local = set(snake)
        for _ in range(NUM_FOODS):
            food = make_food(blocked_local)
            foods.append(food)
            blocked_local.add(food["pos"])

        score = 0
        game_over = False

        current_head_color, current_tail_color = random_worm_colors()
        palette_start_head = current_head_color
        palette_start_tail = current_tail_color
        palette_target_head = current_head_color
        palette_target_tail = current_tail_color
        palette_t = 1.0

    def handle_game_over():
        nonlocal game_over, user_best, scores
        if game_over:
            return  # already handled
        # stop move sound
        if sound_move is not None:
            sound_move.stop()
        # crash sound
        if sound_crash is not None:
            sound_crash.play()
        # update scores
        data = update_user_scores(scores, username, score)
        user_best = data.get("best", user_best)
        save_scores(scores)
        game_over = True

    while running:
        clock.tick(FPS)
        frame_count += 1

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False

            elif event.type == pygame.KEYDOWN:
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

                if game_over and event.key == pygame.K_r:
                    reset_game_state()
                    if sound_move is not None:
                        sound_move.stop()
                        sound_move.play(loops=-1)

        if not game_over:
            # Move snake one cell
            head_x, head_y = snake[0]
            dx, dy = direction
            new_head = (head_x + dx, head_y + dy)

            # Collision with walls?
            if (
                new_head[0] < 0
                or new_head[0] >= GRID_COLS
                or new_head[1] < 0
                or new_head[1] >= GRID_ROWS
            ):
                handle_game_over()

            # Collision with itself?
            elif new_head in snake:
                handle_game_over()

            else:
                # Add new head
                snake.insert(0, new_head)

                # Did we eat any food?
                eaten_index = None
                for i, food in enumerate(foods):
                    if new_head == food["pos"]:
                        eaten_index = i
                        break

                if eaten_index is not None:
                    score += 1

                    if sound_eat is not None:
                        sound_eat.play()
                    if sound_grow is not None:
                        sound_grow.play()

                    # Start new color palette fade
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

                    # Remove eaten food & spawn a new one
                    foods.pop(eaten_index)
                    blocked = set(snake) | {f["pos"] for f in foods}
                    new_food = make_food(blocked)
                    foods.append(new_food)

                    # Grow: don't pop tail

                else:
                    # No food eaten: keep length same
                    snake.pop()

                # Update food timers & handle expired foods
                if not game_over:
                    blocked = set(snake) | {f["pos"] for f in foods}
                    alarm_threshold_frames = int(FOOD_ALARM_TIME * FPS)

                    i = 0
                    while i < len(foods):
                        foods[i]["time_left"] -= 1
                        tl = foods[i]["time_left"]

                        # Alarm when about to expire (only once)
                        if (
                            tl <= alarm_threshold_frames
                            and tl > 0
                            and not foods[i].get("alarm_played", False)
                        ):
                            if sound_alarm is not None:
                                sound_alarm.play()
                            foods[i]["alarm_played"] = True
                            foods[i]["color"] = RED  # change color to red

                        # Expired?
                        if tl <= 0:
                            # Food expired: pop sound, lose 1 point
                            if sound_pop is not None:
                                sound_pop.play()

                            score -= 1

                            # If score drops below zero, end the game with crash sound
                            if score < 0:
                                handle_game_over()
                                break

                            # Otherwise, respawn this food
                            expired = foods.pop(i)
                            blocked.discard(expired["pos"])

                            new_food = make_food(blocked)
                            foods.append(new_food)
                            blocked.add(new_food["pos"])

                            if sound_spawn is not None:
                                sound_spawn.play()
                            # don't increment i, list compacted
                        else:
                            i += 1

        # Update palette fade
        if palette_t < 1.0:
            palette_t += palette_fade_speed
            if palette_t > 1.0:
                palette_t = 1.0

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
        draw_foods(screen, foods)

        # HUD
        draw_text(screen, f"Player: {username}", 20, WHITE, (130, 15))
        draw_text(screen, f"Score: {score}", 20, WHITE, (90, 40))
        draw_text(screen, f"Best: {user_best}", 20, WHITE, (90, 65))

        if game_over:
            draw_text(screen, "GAME OVER", 40, WHITE, (WIDTH // 2, HEIGHT // 2 - 20))
            draw_text(
                screen,
                "Press R to restart",
                20,
                WHITE,
                (WIDTH // 2, HEIGHT // 2 + 20),
            )

        pygame.display.flip()

    pygame.quit()
    sys.exit()


if __name__ == "__main__":
    username = get_username_pygame()
    run_game(username)
