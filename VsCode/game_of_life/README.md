# Conway's Game of Life - Custom Deluxe

This version builds on the advanced and super-advanced versions with even more features:

## Features

### Simulation
- `SPACE` – pause / run
- `N` – step one generation (only when paused)
- `R` – randomize world
- `C` – clear world
- `T` – toggle wrap edges ON/OFF
- `UP` / `DOWN` – speed up / slow down (generations per second)

### Camera & Zoom (Big Scrollable World)
- World size: 220×220 cells
- `W` / `A` / `S` / `D` – pan camera up / left / down / right
- `Q` – zoom out
- `E` – zoom in
- `0` – reset zoom and recenter camera

### Drawing & Editing
- Click (when **paused**) – toggle a cell
- Click & drag (paused) – "paint" live cells

### Visuals
- Color-coded **cell age**: younger cells use the base alive color; older cells fade toward white
- Toggle age-color mode with `Y`
- Toggle grid lines with `G`

### Themes (H to cycle)
- `H` – switch through color themes:
  - Classic Green
  - Neon Blue
  - Warm Amber
  - Monochrome

### Patterns
- Press a number key, pattern is stamped at the mouse position:
  - `1` – Glider
  - `2` – Small exploder
  - `3` – Lightweight spaceship (LWSS)
  - `4` – Gosper glider gun (large pattern)

### Save / Load
- `F5` – save current grid to `pattern.json`
- `F9` – load from `pattern.json`
- NOTE: The pattern must match the configured grid size (220×220).

## Requirements

- Python 3.8+
- Pygame

Install dependencies:

```bash
pip install pygame
```

## Run the game

```bash
python game_of_life_custom.py
```

Experiment with different themes, patterns, and speeds to explore rich emergent behavior.
