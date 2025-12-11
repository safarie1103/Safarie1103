import pygame
import random
import sys
import math
import json
import os

# --------------------
# Config
# --------------------
CELL_SIZE = 20  # size of each grid cell in pixels
GRID_COLS = 60  # number of columns
GRID_ROWS = 40  # number of rows
WIDTH = GRID_COLS * CELL_SIZE
HEIGHT = GRID_ROWS * CELL_SIZE

FPS = 12  # render / logic fps

FOOD_LIFETIME = 30.0  # seconds
FOOD_ALARM_TIME = 10.0  # seconds before expiry

SCORES_FILE = "worm_scores.json"

# Colors
BLACK = (0, 0, 0)
DARK_GREY = (40, 40, 40)
WHITE = (255, 255, 255)
RED = (255, 0, 0)


# --------------------
# Utility / drawing helpers
# --------------------
def random_grid_position(blocked):
    """Return a random (x, y) not in blocked set."""
    while True:
        x = random.randrange(0, GRID_COLS)
        y = random.randrange(0, GRID_ROWS)
        if (x, y) not in blocked:
            return x, y


def draw_grid(surface):
    for x in range(0, WIDTH, CELL_SIZE):
        pygame.draw.line(surface, DARK_GREY, (x, 0), (x, HEIGHT))
    for y in range(0, HEIGHT, CELL_SIZE):
        pygame.draw.line(surface, DARK_GREY, (0, y), (WIDTH, y))


def lerp_color(c1, c2, t):
    return (
        int(c1[0] + (c2[0] - c1[0]) * t),
        int(c1[1] + (c2[1] - c1[1]) * t),
        int(c1[2] + (c2[2] - c1[2]) * t),
    )


def random_worm_colors():
    r = random.randint(80, 255)
    g = random.randint(80, 255)
    b = random.randint(80, 255)
    head = (r, g, b)
    tail = (int(r * 0.5), int(g * 0.5), int(b * 0.5))
    return head, tail


def random_food_color():
    return (
        random.randint(80, 255),
        random.randint(80, 255),
        random.randint(80, 255),
    )


def make_food(blocked):
    pos = random_grid_position(blocked)
    color = random_food_color()
    time_left = int(FOOD_LIFETIME * FPS)  # in frames
    return {
        "pos": pos,
        "color": color,
        "time_left": time_left,
        "alarm_played": False,
    }


def draw_worm_eyes(surface, cx, cy, radius, logical_dir):
    dx, dy = logical_dir
    if dx == 0 and dy == 0:
        dx, dy = 1, 0

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
    if not snake:
        return

    wave_amplitude = CELL_SIZE * 0.20
    wave_speed = 0.25
    wave_segment_spacing = 0.6

    base_radius = CELL_SIZE * 0.45
    min_radius = CELL_SIZE * 0.25

    for i, (gx, gy) in enumerate(snake):
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

        if len(snake) > 1:
            t = i / (len(snake) - 1)
        else:
            t = 0.0
        radius = base_radius * (1 - 0.5 * t)
        radius = max(min_radius, radius)

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


# --------------------
# Particles
# --------------------
def spawn_particles(particles, x_px, y_px, base_color, count=15, speed=4, life=20):
    """Spawn simple circular particles around a point (pixel coords)."""
    for _ in range(count):
        angle = random.uniform(0, 2 * math.pi)
        vel = random.uniform(0.5, speed)
        vx = math.cos(angle) * vel
        vy = math.sin(angle) * vel
        # Slight color variation
        jitter = lambda c: max(0, min(255, c + random.randint(-40, 40)))
        color = (jitter(base_color[0]), jitter(base_color[1]), jitter(base_color[2]))
        particles.append(
            {
                "x": x_px,
                "y": y_px,
                "vx": vx,
                "vy": vy,
                "radius": random.uniform(3, 6),
                "color": color,
                "life": life,
            }
        )


def update_and_draw_particles(surface, particles):
    i = 0
    while i < len(particles):
        p = particles[i]
        p["x"] += p["vx"]
        p["y"] += p["vy"]
        p["life"] -= 1
        p["radius"] *= 0.95

        if p["life"] <= 0 or p["radius"] <= 0.5:
            particles.pop(i)
            continue

        pygame.draw.circle(
            surface,
            p["color"],
            (int(p["x"]), int(p["y"])),
            int(p["radius"]),
        )
        i += 1


# --------------------
# Scores / leaderboard
# --------------------
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


def get_global_leaderboard(scores, top_n=10):
    items = []
    for name, data in scores.items():
        best = data.get("best", 0)
        count = len(data.get("scores", []))
        items.append((name, best, count))
    items.sort(key=lambda t: t[1], reverse=True)
    return items[:top_n]


# --------------------
# Menus / UI screens
# --------------------
def show_main_menu(screen, clock, settings):
    """Return 'play', 'leaderboard', 'options', or 'quit'."""
    title_font = pygame.font.SysFont("consolas", 48, bold=True)
    opt_font = pygame.font.SysFont("consolas", 28)
    info_font = pygame.font.SysFont("consolas", 20)

    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return "quit"
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    return "quit"
                elif event.key in (pygame.K_p, pygame.K_RETURN, pygame.K_SPACE):
                    return "play"
                elif event.key == pygame.K_l:
                    return "leaderboard"
                elif event.key == pygame.K_o:
                    return "options"

        screen.fill((5, 5, 15))
        title_surf = title_font.render("REAL WORM", True, WHITE)
        screen.blit(
            title_surf, title_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 100))
        )

        opt_play = opt_font.render("[P] Play", True, WHITE)
        opt_lead = opt_font.render("[L] Leaderboard", True, WHITE)
        opt_opt = opt_font.render("[O] Options", True, WHITE)
        opt_quit = opt_font.render("[Esc] Quit", True, WHITE)

        screen.blit(opt_play, opt_play.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 10)))
        screen.blit(opt_lead, opt_lead.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 30)))
        screen.blit(opt_opt, opt_opt.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 70)))
        screen.blit(opt_quit, opt_quit.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 110)))

        # Show current settings summary
        settings_text = f"Speed: {settings['speed']}  |  Foods: {settings['foods']}"
        settings_surf = info_font.render(settings_text, True, (200, 200, 200))
        screen.blit(
            settings_surf, settings_surf.get_rect(center=(WIDTH // 2, HEIGHT - 80))
        )

        info = info_font.render(
            "Arrow keys / WASD to move. Esc always goes back.", True, (180, 180, 180)
        )
        screen.blit(info, info.get_rect(center=(WIDTH // 2, HEIGHT - 40)))

        pygame.display.flip()


def show_leaderboard_screen(screen, clock, scores):
    title_font = pygame.font.SysFont("consolas", 40, bold=True)
    row_font = pygame.font.SysFont("consolas", 26)
    info_font = pygame.font.SysFont("consolas", 20)

    leaderboard = get_global_leaderboard(scores)

    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return
            elif event.type == pygame.KEYDOWN:
                if event.key in (pygame.K_ESCAPE, pygame.K_RETURN, pygame.K_SPACE):
                    return

        screen.fill((5, 5, 20))
        title_surf = title_font.render("LEADERBOARD (Best Scores)", True, WHITE)
        screen.blit(title_surf, title_surf.get_rect(center=(WIDTH // 2, 80)))

        if not leaderboard:
            txt = row_font.render("No scores yet. Go play!", True, WHITE)
            screen.blit(txt, txt.get_rect(center=(WIDTH // 2, HEIGHT // 2)))
        else:
            start_y = 150
            for idx, (name, best, count) in enumerate(leaderboard, start=1):
                line = f"{idx}. {name}  -  {best}  (games: {count})"
                line_surf = row_font.render(line, True, WHITE)
                screen.blit(line_surf, (WIDTH // 2 - 320, start_y))
                start_y += 32

        info = info_font.render(
            "Press Esc / Enter / Space to return", True, (180, 180, 180)
        )
        screen.blit(info, info.get_rect(center=(WIDTH // 2, HEIGHT - 40)))

        pygame.display.flip()


def show_options_screen(screen, clock, settings):
    title_font = pygame.font.SysFont("consolas", 40, bold=True)
    row_font = pygame.font.SysFont("consolas", 26)
    info_font = pygame.font.SysFont("consolas", 20)

    speed_modes = ["Slow", "Normal", "Fast", " Superfast", "Superdupperfast"]
    selected_index = 0  # 0 = speed, 1 = foods

    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return
            elif event.type == pygame.KEYDOWN:
                if event.key in (pygame.K_ESCAPE, pygame.K_RETURN, pygame.K_SPACE):
                    return
                elif event.key == pygame.K_UP or event.key == pygame.K_DOWN:
                    selected_index = 1 - selected_index  # toggle between 0 and 1
                elif event.key == pygame.K_LEFT:
                    if selected_index == 0:
                        # speed
                        curr_i = speed_modes.index(settings["speed"])
                        settings["speed"] = speed_modes[(curr_i - 1) % len(speed_modes)]
                    else:
                        # foods
                        settings["foods"] = max(1, settings["foods"] - 1)
                elif event.key == pygame.K_RIGHT:
                    if selected_index == 0:
                        curr_i = speed_modes.index(settings["speed"])
                        settings["speed"] = speed_modes[(curr_i + 1) % len(speed_modes)]
                    else:
                        settings["foods"] = min(20, settings["foods"] + 1)

        screen.fill((5, 10, 25))
        title_surf = title_font.render("OPTIONS", True, WHITE)
        screen.blit(title_surf, title_surf.get_rect(center=(WIDTH // 2, 80)))

        # Speed row
        speed_label = f"Speed: {settings['speed']}  (Slow / Normal / Fast / Superfast / Superdupperfast)"
        color_speed = (255, 255, 255) if selected_index == 0 else (200, 200, 200)
        speed_surf = row_font.render(speed_label, True, color_speed)
        screen.blit(
            speed_surf, speed_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 20))
        )

        # Foods row
        foods_label = f"Number of foods: {settings['foods']}  (1 - 20)"
        color_foods = (255, 255, 255) if selected_index == 1 else (200, 200, 200)
        foods_surf = row_font.render(foods_label, True, color_foods)
        screen.blit(
            foods_surf, foods_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 30))
        )

        info = info_font.render(
            "Up/Down: select  |  Left/Right: change  |  Esc/Enter: back",
            True,
            (180, 180, 180),
        )
        screen.blit(info, info.get_rect(center=(WIDTH // 2, HEIGHT - 40)))

        pygame.display.flip()


def get_username_pygame(screen, clock):
    font = pygame.font.SysFont("consolas", 36, bold=True)
    small_font = pygame.font.SysFont("consolas", 22)

    username = ""
    while True:
        clock.tick(FPS)
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return None
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_RETURN:
                    return username.strip() or "Player"
                elif event.key == pygame.K_BACKSPACE:
                    username = username[:-1]
                elif event.key == pygame.K_ESCAPE:
                    return None
                else:
                    if event.unicode and event.unicode.isprintable():
                        if len(username) < 16:
                            username += event.unicode

        screen.fill((0, 0, 0))
        prompt_surf = font.render("Enter your name:", True, WHITE)
        screen.blit(
            prompt_surf, prompt_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 60))
        )

        display_name = username if username else "_"
        name_surf = font.render(display_name, True, WHITE)
        screen.blit(name_surf, name_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2)))

        info_surf = small_font.render(
            "Press Enter to start  |  Esc to cancel", True, WHITE
        )
        screen.blit(
            info_surf, info_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 60))
        )

        pygame.display.flip()


def show_countdown(screen, clock, username):
    big_font = pygame.font.SysFont("consolas", 60, bold=True)
    small_font = pygame.font.SysFont("consolas", 28)

    for n in range(3, 0, -1):
        start_ticks = pygame.time.get_ticks()
        while pygame.time.get_ticks() - start_ticks < 1000:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    return False
                elif event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
                    return False

            screen.fill(BLACK)
            msg = f"Get ready, {username}!"
            msg_surf = small_font.render(msg, True, WHITE)
            screen.blit(
                msg_surf, msg_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 - 50))
            )

            num_surf = big_font.render(str(n), True, WHITE)
            screen.blit(
                num_surf, num_surf.get_rect(center=(WIDTH // 2, HEIGHT // 2 + 10))
            )

            pygame.display.flip()
            clock.tick(FPS)
    return True


# --------------------
# Audio
# --------------------
def init_sounds():
    sounds = {
        "move": None,
        "eat": None,
        "grow": None,
        "crash": None,
        "pop": None,
        "spawn": None,
        "alarm": None,
    }
    try:
        pygame.mixer.init()
        sounds["move"] = pygame.mixer.Sound("move_loop.wav")
        sounds["eat"] = pygame.mixer.Sound("eat.wav")
        sounds["grow"] = pygame.mixer.Sound("grow.wav")
        sounds["crash"] = pygame.mixer.Sound("crash.wav")
        sounds["pop"] = pygame.mixer.Sound("pop.wav")
        sounds["spawn"] = pygame.mixer.Sound("spawn.wav")
        sounds["alarm"] = pygame.mixer.Sound("alarm.wav")

        if sounds["move"]:
            sounds["move"].set_volume(0.4)
        if sounds["eat"]:
            sounds["eat"].set_volume(0.7)
        if sounds["grow"]:
            sounds["grow"].set_volume(0.7)
        if sounds["crash"]:
            sounds["crash"].set_volume(0.8)
        if sounds["pop"]:
            sounds["pop"].set_volume(0.7)
        if sounds["spawn"]:
            sounds["spawn"].set_volume(0.7)
        if sounds["alarm"]:
            sounds["alarm"].set_volume(0.7)
    except Exception as e:
        print("Audio init failed or sounds missing:", e)
    return sounds


# --------------------
# Game loop
# --------------------
def run_game(
    screen, clock, username, scores, sounds, base_move_delay, min_move_delay, num_foods
):
    # Score info
    user_data = scores.get(username, {"scores": [], "best": 0})
    user_best = user_data.get("best", 0)

    # Snake and foods
    snake = [
        (GRID_COLS // 2, GRID_ROWS // 2),
        (GRID_COLS // 2 - 1, GRID_ROWS // 2),
        (GRID_COLS // 2 - 2, GRID_ROWS // 2),
    ]
    direction = (1, 0)

    foods = []
    blocked = set(snake)
    for _ in range(num_foods):
        f = make_food(blocked)
        foods.append(f)
        blocked.add(f["pos"])

    score = 0
    game_over = False
    frame_count = 0

    # Particles
    particles = []

    # Movement speed
    move_delay_frames = base_move_delay
    move_counter = 0

    # Color palette fade
    current_head_color, current_tail_color = random_worm_colors()
    palette_start_head = current_head_color
    palette_start_tail = current_tail_color
    palette_target_head = current_head_color
    palette_target_tail = current_tail_color
    palette_t = 1.0
    palette_fade_speed = 0.03

    # Start countdown
    if not show_countdown(screen, clock, username):
        return  # aborted, back to menu

    # Start looping move sound
    if sounds["move"] is not None:
        sounds["move"].play(loops=-1)

    def recalc_move_delay():
        nonlocal move_delay_frames
        # Speed up as score increases: every +3 score reduce delay by 1 frame
        move_delay_frames = max(min_move_delay, base_move_delay - score // 3)

    def reset_game_state():
        nonlocal snake, direction, foods, score, game_over
        nonlocal current_head_color, current_tail_color
        nonlocal palette_start_head, palette_start_tail
        nonlocal palette_target_head, palette_target_tail, palette_t
        nonlocal move_delay_frames, move_counter, particles

        snake = [
            (GRID_COLS // 2, GRID_ROWS // 2),
            (GRID_COLS // 2 - 1, GRID_ROWS // 2),
            (GRID_COLS // 2 - 2, GRID_ROWS // 2),
        ]
        direction = (1, 0)

        foods = []
        blocked_local = set(snake)
        for _ in range(num_foods):
            f = make_food(blocked_local)
            foods.append(f)
            blocked_local.add(f["pos"])

        score = 0
        game_over = False
        particles = []

        current_head_color, current_tail_color = random_worm_colors()
        palette_start_head = current_head_color
        palette_start_tail = current_tail_color
        palette_target_head = current_head_color
        palette_target_tail = current_tail_color
        palette_t = 1.0

        move_delay_frames = base_move_delay
        move_counter = 0

        recalc_move_delay()

        # restart move sound
        if sounds["move"] is not None:
            sounds["move"].stop()
            sounds["move"].play(loops=-1)

    def handle_game_over():
        nonlocal game_over, user_best, scores
        if game_over:
            return
        if sounds["move"] is not None:
            sounds["move"].stop()
        if sounds["crash"] is not None:
            sounds["crash"].play()

        # crash particles at head
        hx, hy = snake[0]
        hx_px = hx * CELL_SIZE + CELL_SIZE / 2
        hy_px = hy * CELL_SIZE + CELL_SIZE / 2
        spawn_particles(
            particles, hx_px, hy_px, (255, 80, 80), count=40, speed=6, life=35
        )

        data = update_user_scores(scores, username, score)
        user_best = data.get("best", user_best)
        save_scores(scores)
        game_over = True

    recalc_move_delay()

    # Main game loop
    running = True
    while running:
        clock.tick(FPS)
        frame_count += 1
        move_counter += 1

        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
                # stop sounds
                if sounds["move"] is not None:
                    sounds["move"].stop()
            elif event.type == pygame.KEYDOWN:
                if event.key == pygame.K_ESCAPE:
                    # Exit back to menu (no game_over handling)
                    if sounds["move"] is not None:
                        sounds["move"].stop()
                    running = False
                elif event.key in (pygame.K_RIGHT, pygame.K_d):
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

        # Logic update only if not game_over
        if not game_over:
            # Move snake on interval
            if move_counter >= move_delay_frames:
                move_counter = 0

                head_x, head_y = snake[0]
                dx, dy = direction
                new_head = (head_x + dx, head_y + dy)

                # Check walls
                if (
                    new_head[0] < 0
                    or new_head[0] >= GRID_COLS
                    or new_head[1] < 0
                    or new_head[1] >= GRID_ROWS
                ):
                    handle_game_over()
                # Self collision
                elif new_head in snake:
                    handle_game_over()
                else:
                    snake.insert(0, new_head)

                    # Check food
                    eaten_index = None
                    for i, food in enumerate(foods):
                        if new_head == food["pos"]:
                            eaten_index = i
                            break

                    if eaten_index is not None:
                        score += 1
                        recalc_move_delay()

                        if sounds["eat"] is not None:
                            sounds["eat"].play()
                        if sounds["grow"] is not None:
                            sounds["grow"].play()

                        # particle burst at head (using food color)
                        fx, fy = foods[eaten_index]["pos"]
                        fx_px = fx * CELL_SIZE + CELL_SIZE / 2
                        fy_px = fy * CELL_SIZE + CELL_SIZE / 2
                        spawn_particles(
                            particles,
                            fx_px,
                            fy_px,
                            foods[eaten_index]["color"],
                            count=20,
                            speed=5,
                            life=30,
                        )

                        # start new color palette fade
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

                        # Remove eaten food and spawn new one
                        foods.pop(eaten_index)
                        blocked = set(snake) | {f["pos"] for f in foods}
                        new_f = make_food(blocked)
                        foods.append(new_f)

                        if sounds["spawn"] is not None:
                            sounds["spawn"].play()

                        # Grow: don't pop tail
                    else:
                        # No food: move without growing
                        snake.pop()

            # Update food timers regardless of whether moved
            if not game_over:
                blocked = set(snake) | {f["pos"] for f in foods}
                alarm_threshold_frames = int(FOOD_ALARM_TIME * FPS)

                i = 0
                while i < len(foods):
                    foods[i]["time_left"] -= 1
                    tl = foods[i]["time_left"]

                    # Alarm when close to expiring
                    if (
                        tl <= alarm_threshold_frames
                        and tl > 0
                        and not foods[i].get("alarm_played", False)
                    ):
                        if sounds["alarm"] is not None:
                            sounds["alarm"].play()
                        foods[i]["alarm_played"] = True
                        foods[i]["color"] = RED  # change color to red

                    # Expired
                    if tl <= 0:
                        if sounds["pop"] is not None:
                            sounds["pop"].play()

                        score -= 1
                        recalc_move_delay()

                        # Penalty: if score < 0 -> crash
                        if score < 0:
                            handle_game_over()
                            break

                        expired = foods.pop(i)
                        blocked.discard(expired["pos"])

                        new_f = make_food(blocked)
                        foods.append(new_f)
                        blocked.add(new_f["pos"])

                        if sounds["spawn"] is not None:
                            sounds["spawn"].play()
                    else:
                        i += 1

        # Palette fade update
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
        update_and_draw_particles(screen, particles)

        # HUD
        draw_text(screen, f"Player: {username}", 22, WHITE, (140, 20))
        draw_text(screen, f"Score: {score}", 22, WHITE, (110, 50))
        draw_text(screen, f"Best: {user_best}", 22, WHITE, (110, 80))

        draw_text(
            screen,
            "Esc: menu  |  R: restart (after crash)",
            18,
            WHITE,
            (WIDTH - 260, 20),
        )

        pygame.display.flip()


# --------------------
# Main
# --------------------
def main():
    pygame.init()
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    pygame.display.set_caption("Real Worm – 2D Deluxe")
    clock = pygame.time.Clock()

    scores = load_scores()
    sounds = init_sounds()

    # Game settings (user adjustable)
    settings = {
        "speed": "Normal",  # Slow / Normal / Fast
        "foods": 2,  # number of foods on screen
    }

    running = True
    while running:
        choice = show_main_menu(screen, clock, settings)
        if choice == "quit":
            running = False
        elif choice == "leaderboard":
            scores = load_scores()
            show_leaderboard_screen(screen, clock, scores)
        elif choice == "options":
            show_options_screen(screen, clock, settings)
        elif choice == "play":
            username = get_username_pygame(screen, clock)
            if username is None:
                continue
            # determine speed settings
            if settings["speed"] == "Slow":
                base_move_delay = 12
                min_move_delay = 6
            elif settings["speed"] == "Fast":
                base_move_delay = 5
                min_move_delay = 2
            elif settings["speed"] == "Superfast":
                base_move_delay = 3
                min_move_delay = 1
            elif settings["speed"] == "Superdupperfast":
                base_move_delay = 1
                min_move_delay = 1
            else:  # Normal
                base_move_delay = 8
                min_move_delay = 3

            num_foods = settings["foods"]

            scores = load_scores()
            run_game(
                screen,
                clock,
                username,
                scores,
                sounds,
                base_move_delay,
                min_move_delay,
                num_foods,
            )

    pygame.quit()
    sys.exit()


if __name__ == "__main__":
    main()
