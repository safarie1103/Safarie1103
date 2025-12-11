import curses
import random
import os

HIGHSCORE_FILE = "worm_highscore.txt"


def load_highscore():
    if not os.path.exists(HIGHSCORE_FILE):
        return 0
    try:
        with open(HIGHSCORE_FILE, "r") as f:
            return int(f.read().strip() or 0)
    except Exception:
        return 0


def save_highscore(score):
    try:
        with open(HIGHSCORE_FILE, "w") as f:
            f.write(str(score))
    except Exception:
        pass


def draw_centered(stdscr, y, text, attr=0):
    h, w = stdscr.getmaxyx()
    x = max(0, (w - len(text)) // 2)
    stdscr.addstr(y, x, text, attr)


def show_menu(stdscr, highscore):
    stdscr.clear()
    stdscr.nodelay(False)
    curses.curs_set(0)

    h, w = stdscr.getmaxyx()

    title = "WORM GAME"
    subtitle = "Use ↑/↓ and Enter to select"
    draw_centered(stdscr, h // 2 - 4, title, curses.A_BOLD)
    draw_centered(stdscr, h // 2 - 2, f"High Score: {highscore}", curses.A_DIM)
    draw_centered(stdscr, h // 2 - 1, subtitle)

    menu_items = ["Easy", "Normal", "Hard", "Quit"]
    current_idx = 1  # default to Normal

    while True:
        for i, item in enumerate(menu_items):
            y = h // 2 + i + 1
            prefix = "> " if i == current_idx else "  "
            attr = curses.A_REVERSE if i == current_idx else 0
            draw_centered(stdscr, y, prefix + item, attr)

        key = stdscr.getch()
        if key in (curses.KEY_UP, ord("k")):
            current_idx = (current_idx - 1) % len(menu_items)
        elif key in (curses.KEY_DOWN, ord("j")):
            current_idx = (current_idx + 1) % len(menu_items)
        elif key in (curses.KEY_ENTER, 10, 13):
            return menu_items[current_idx]
        elif key in (ord("q"), ord("Q")):
            return "Quit"


def play_game(stdscr, speed_ms, highscore):
    curses.curs_set(0)
    sh, sw = stdscr.getmaxyx()

    win = curses.newwin(sh, sw, 0, 0)
    win.keypad(True)
    win.nodelay(True)
    win.timeout(speed_ms)

    # Colors
    # 1: snake, 2: food, 3: border, 4: text
    win.attron(curses.color_pair(3))
    win.border()
    win.attroff(curses.color_pair(3))

    snk_y = sh // 2
    snk_x = sw // 4
    snake = [
        [snk_y, snk_x],
        [snk_y, snk_x - 1],
        [snk_y, snk_x - 2],
    ]

    food = [sh // 2, sw // 2]
    win.attron(curses.color_pair(2))
    win.addch(food[0], food[1], curses.ACS_PI)
    win.attroff(curses.color_pair(2))

    key = curses.KEY_RIGHT
    score = 0

    while True:
        # Draw header
        win.attron(curses.color_pair(4))
        win.addstr(0, 2, f" Score: {score} ")
        win.addstr(0, 20, f" High: {highscore} ")
        win.addstr(0, sw - 18, " Q to quit ")
        win.attroff(curses.color_pair(4))

        next_key = win.getch()

        if next_key in (ord("q"), ord("Q")):
            return score

        if next_key in [
            curses.KEY_RIGHT,
            curses.KEY_LEFT,
            curses.KEY_UP,
            curses.KEY_DOWN,
        ]:
            opposite = {
                curses.KEY_RIGHT: curses.KEY_LEFT,
                curses.KEY_LEFT: curses.KEY_RIGHT,
                curses.KEY_UP: curses.KEY_DOWN,
                curses.KEY_DOWN: curses.KEY_UP,
            }
            if next_key != opposite.get(key, None):
                key = next_key

        head = snake[0].copy()
        if key == curses.KEY_RIGHT:
            head[1] += 1
        elif key == curses.KEY_LEFT:
            head[1] -= 1
        elif key == curses.KEY_UP:
            head[0] -= 1
        elif key == curses.KEY_DOWN:
            head[0] += 1

        # Collision: walls or self
        if head[0] in [0, sh - 1] or head[1] in [0, sw - 1] or head in snake:
            msg = " GAME OVER! "
            win.attron(curses.color_pair(4) | curses.A_BOLD)
            win.addstr(sh // 2, (sw - len(msg)) // 2, msg)
            win.attroff(curses.color_pair(4) | curses.A_BOLD)
            win.nodelay(False)
            win.getch()
            return score

        snake.insert(0, head)

        if head == food:
            score += 1
            while True:
                new_food = [
                    random.randint(1, sh - 2),
                    random.randint(1, sw - 2),
                ]
                if new_food not in snake:
                    food = new_food
                    break
            win.attron(curses.color_pair(2))
            win.addch(food[0], food[1], curses.ACS_PI)
            win.attroff(curses.color_pair(2))
        else:
            tail = snake.pop()
            win.addch(tail[0], tail[1], " ")

        # Draw snake
        win.attron(curses.color_pair(1))
        win.addch(snake[0][0], snake[0][1], "#")
        win.attroff(curses.color_pair(1))


def game_over_screen(stdscr, score, highscore, new_high):
    stdscr.clear()
    stdscr.nodelay(False)
    h, w = stdscr.getmaxyx()

    title = "GAME OVER"
    draw_centered(stdscr, h // 2 - 3, title, curses.A_BOLD)

    draw_centered(stdscr, h // 2 - 1, f"Score: {score}")
    draw_centered(stdscr, h // 2, f"High Score: {highscore}")

    if new_high:
        draw_centered(
            stdscr, h // 2 + 2, "NEW HIGH SCORE!", curses.A_BLINK | curses.A_BOLD
        )

    draw_centered(stdscr, h // 2 + 4, "Play again? (y/n)")

    while True:
        key = stdscr.getch()
        if key in (ord("y"), ord("Y")):
            return True
        elif key in (ord("n"), ord("N"), ord("q"), ord("Q")):
            return False


def main(stdscr):
    curses.curs_set(0)
    curses.start_color()
    curses.use_default_colors()

    # Define color pairs
    curses.init_pair(1, curses.COLOR_GREEN, -1)  # snake
    curses.init_pair(2, curses.COLOR_RED, -1)  # food
    curses.init_pair(3, curses.COLOR_WHITE, -1)  # border
    curses.init_pair(4, curses.COLOR_CYAN, -1)  # text

    highscore = load_highscore()

    while True:
        choice = show_menu(stdscr, highscore)
        if choice == "Quit":
            break

        # Map difficulty to speed
        if choice == "Easy":
            speed = 150
        elif choice == "Normal":
            speed = 100
        else:  # Hard
            speed = 60

        score = play_game(stdscr, speed, highscore)

        new_high = False
        if score > highscore:
            highscore = score
            save_highscore(highscore)
            new_high = True

        again = game_over_screen(stdscr, score, highscore, new_high)
        if not again:
            break


if __name__ == "__main__":
    curses.wrapper(main)
