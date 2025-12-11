# Conway's Game of Life - Super Advanced

This version includes:

- Pause / run (`SPACE`)
- Single-step when paused (`N`)
- Randomize (`R`) and clear (`C`)
- Optional edge wrapping (`T`)
- Color-coded cell age (older cells are brighter)
- Big world with camera:
  - Pan with `I` (up), `K` (down), `J` (left), `L` (right)
  - Zoom with `Q` (out) and `E` (in)
- Speed control with `UP` / `DOWN` arrows
- Pattern stamping at mouse position:
  - `1` = glider
  - `2` = small exploder
- Save/load full-grid patterns:
  - `F5` = save to `pattern.json`
  - `F9` = load from `pattern.json`

## Requirements

- Python 3.8+
- Pygame

Install dependencies:

```bash
pip install pygame
```

## Run the game

```bash
python game_of_life_super_advanced.py
```

Then use the keys listed above to play with the world.
