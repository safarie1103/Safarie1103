import curses
import random


def main(stdscr):
    # Turn off the cursor
    curses.curs_set(0)

    # Get screen height and width
    sh, sw = stdscr.getmaxyx()

    # Create a new window for the game
    win = curses.newwin(sh, sw, 0, 0)
    win.keypad(True)          # Enable arrow keys
    win.nodelay(True)         # Don't block on getch()
    win.timeout(100)          # Refresh every 100 ms

    # Draw border
    win.border()

    # Initial snake setup (y, x)
    snk_y = sh // 2
    snk_x = sw // 4
    snake = [
        [snk_y, snk_x],
        [snk_y, snk_x - 1],
        [snk_y, snk_x - 2],
    ]

    # Initial food position
    food = [sh // 2, sw // 2]
    win.addch(food[0], food[1], curses.ACS_PI)

    # Initial direction (moving right)
    key = curses.KEY_RIGHT

    score = 0

    while True:
        # Display score in top-left
        win.addstr(0, 2, f" Score: {score} ")

        # Get user input
        next_key = win.getch()

        # Allow quitting with 'q'
        if next_key in (ord('q'), ord('Q')):
            break

        # Only update direction if a valid arrow key is pressed
        if next_key in [curses.KEY_RIGHT, curses.KEY_LEFT, curses.KEY_UP, curses.KEY_DOWN]:
            # Prevent 180-degree turns (no instant reverse)
            opposite = {
                curses.KEY_RIGHT: curses.KEY_LEFT,
                curses.KEY_LEFT: curses.KEY_RIGHT,
                curses.KEY_UP: curses.KEY_DOWN,
                curses.KEY_DOWN: curses.KEY_UP,
            }
            if next_key != opposite.get(key, None):
                key = next_key

        # Compute new head position
        head = snake[0].copy()
        if key == curses.KEY_RIGHT:
            head[1] += 1
        elif key == curses.KEY_LEFT:
            head[1] -= 1
        elif key == curses.KEY_UP:
            head[0] -= 1
        elif key == curses.KEY_DOWN:
            head[0] += 1

        # Check collisions: walls or self
        if (
            head[0] in [0, sh - 1] or      # top/bottom walls
            head[1] in [0, sw - 1] or      # left/right walls
            head in snake                  # snake runs into itself
        ):
            msg = " GAME OVER! Press any key to exit "
            win.addstr(sh // 2, (sw - len(msg)) // 2, msg)
            win.nodelay(False)
            win.getch()
            break

        # Insert new head
        snake.insert(0, head)

        # Check if we ate food
        if head == food:
            score += 1
            # Place new food at a random empty spot
            while True:
                new_food = [
                    random.randint(1, sh - 2),
                    random.randint(1, sw - 2),
                ]
                if new_food not in snake:
                    food = new_food
                    break
            win.addch(food[0], food[1], curses.ACS_PI)
        else:
            # Remove tail segment
            tail = snake.pop()
            win.addch(tail[0], tail[1], " ")

        # Draw snake head
        win.addch(snake[0][0], snake[0][1], "#")


if __name__ == "__main__":
    curses.wrapper(main)
