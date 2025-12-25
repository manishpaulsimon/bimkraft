#!/usr/bin/env python3
"""
BIM Kraft Icon Generator
========================

Generates power and speed-themed icons for BIMKraft tools.

Usage:
    python generate_icons.py

Requirements:
    pip install pillow

The script will create 32x32 PNG icons with power/speed themed designs:
- Lightning bolts for speed
- Gradients for power
- Dynamic colors and effects
"""

from PIL import Image, ImageDraw, ImageFont
import os

# Configuration
ICON_SIZE = (32, 32)
OUTPUT_DIR = "icons"

# Color schemes (RGB)
ELECTRIC_BLUE = (0, 150, 255)
POWER_ORANGE = (255, 100, 0)
LIGHTNING_YELLOW = (255, 255, 0)
STEEL_GRAY = (100, 120, 140)
ENERGY_GREEN = (50, 200, 50)
FIRE_RED = (255, 50, 0)
GOLD = (255, 215, 0)


def create_gradient_background(draw, size, color1, color2):
    """Create a radial gradient background"""
    center_x, center_y = size[0] // 2, size[1] // 2
    max_radius = min(center_x, center_y)

    for i in range(max_radius, 0, -1):
        # Calculate color interpolation
        ratio = i / max_radius
        r = int(color1[0] * ratio + color2[0] * (1 - ratio))
        g = int(color1[1] * ratio + color2[1] * (1 - ratio))
        b = int(color1[2] * ratio + color2[2] * (1 - ratio))

        # Draw circle
        draw.ellipse(
            [center_x - i, center_y - i, center_x + i, center_y + i],
            fill=(r, g, b, 255)
        )


def draw_lightning_bolt(draw, offset_x=0, offset_y=0, color=LIGHTNING_YELLOW):
    """Draw a lightning bolt symbol"""
    points = [
        (18 + offset_x, 4 + offset_y),
        (14 + offset_x, 14 + offset_y),
        (18 + offset_x, 14 + offset_y),
        (10 + offset_x, 28 + offset_y),
        (16 + offset_x, 18 + offset_y),
        (14 + offset_x, 18 + offset_y)
    ]
    draw.polygon(points, fill=color)

    # Add highlight
    highlight_points = [
        (18 + offset_x, 5 + offset_y),
        (15 + offset_x, 14 + offset_y)
    ]
    draw.line(highlight_points, fill=(255, 255, 255, 200), width=1)


def draw_warning_triangle(draw):
    """Draw a warning triangle"""
    points = [(16, 6), (8, 26), (24, 26)]
    draw.polygon(points, fill=LIGHTNING_YELLOW, outline=(0, 0, 0), width=2)

    # Exclamation mark
    draw.rectangle([14, 12, 18, 20], fill=FIRE_RED)
    draw.ellipse([14, 22, 18, 25], fill=FIRE_RED)


def draw_speed_lines(draw, direction='horizontal'):
    """Draw speed/motion lines"""
    lines = [(2, 8), (2, 12), (2, 16), (26, 8), (26, 12), (26, 16)]
    for i in range(0, len(lines), 2):
        y = lines[i][1]
        draw.line([(2, y), (6, y)], fill=(255, 255, 255, 200), width=2)
        draw.line([(26, y), (30, y)], fill=(255, 255, 255, 200), width=2)


def draw_glossy_border(draw, size):
    """Draw a glossy border effect"""
    # Outer border (dark)
    draw.ellipse([0, 0, size[0]-1, size[1]-1], outline=(50, 50, 50, 100), width=2)

    # Inner glossy highlight
    draw.ellipse([2, 2, size[0]-3, size[1]-3], outline=(255, 255, 255, 150), width=2)


def create_family_renamer_icon():
    """Create Family Renamer icon - Lightning bolt with electric blue"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background
    create_gradient_background(draw, ICON_SIZE, ELECTRIC_BLUE, (0, 75, 128))

    # Lightning bolt
    draw_lightning_bolt(draw, offset_x=0, offset_y=0)

    # Speed lines
    draw_speed_lines(draw)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_warnings_browser_icon():
    """Create Warnings Browser icon - Warning triangle with power orange"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background
    create_gradient_background(draw, ICON_SIZE, POWER_ORANGE, (180, 50, 0))

    # Warning triangle
    draw_warning_triangle(draw)

    # Speed lines
    draw_speed_lines(draw)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_parameter_pro_icon():
    """Create Parameter Pro icon - P with lightning"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Electric blue gradient background
    create_gradient_background(draw, ICON_SIZE, ELECTRIC_BLUE, (0, 75, 128))

    # Draw bold 'P' letter
    draw.rectangle([8, 8, 12, 24], fill=GOLD)
    draw.ellipse([8, 8, 18, 16], fill=GOLD)
    draw.ellipse([12, 10, 16, 14], fill=(0, 75, 128))

    # Lightning bolt accent
    draw_lightning_bolt(draw, offset_x=6, offset_y=4, color=LIGHTNING_YELLOW)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_parameter_transfer_icon():
    """Create Parameter Transfer Pro icon - Arrows with lightning"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Electric blue gradient background
    create_gradient_background(draw, ICON_SIZE, (0, 120, 200), (0, 60, 100))

    # Draw transfer arrows
    # Right arrow
    arrow_right = [(6, 12), (16, 12), (16, 8), (22, 14), (16, 20), (16, 16), (6, 16)]
    draw.polygon(arrow_right, fill=GOLD)

    # Left arrow (smaller, offset)
    arrow_left = [(26, 18), (20, 18), (20, 22), (14, 16), (20, 10), (20, 14), (26, 14)]
    draw.polygon(arrow_left, fill=LIGHTNING_YELLOW)

    # Lightning accent
    draw_lightning_bolt(draw, offset_x=-6, offset_y=0, color=LIGHTNING_YELLOW)

    # Speed lines
    draw_speed_lines(draw)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_workset_manager_icon():
    """Create Workset Manager icon - Layers with power effect"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Steel gray gradient background
    create_gradient_background(draw, ICON_SIZE, STEEL_GRAY, (50, 60, 70))

    # Draw stacked layers with fade effect
    for i, y in enumerate([8, 14, 20]):
        alpha = 255 - i * 40
        draw.rectangle([6, y, 26, y+3], fill=(255, 215, 0, alpha))

    # Add 'W' for Workset
    draw.line([(8, 6), (10, 12), (12, 8), (14, 12), (16, 6)], fill=(255, 255, 255), width=2)

    # Lightning accent
    draw_lightning_bolt(draw, offset_x=-4, offset_y=2, color=LIGHTNING_YELLOW)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_warnings_browser_pro_icon():
    """Create Warnings Browser Pro icon - Enhanced warning triangle"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Power orange gradient background
    create_gradient_background(draw, ICON_SIZE, POWER_ORANGE, (180, 50, 0))

    # Draw warning triangle (larger and more prominent)
    points = [(16, 4), (6, 26), (26, 26)]
    draw.polygon(points, fill=LIGHTNING_YELLOW, outline=(255, 255, 255), width=2)

    # Exclamation mark (bold)
    draw.rectangle([14, 10, 18, 20], fill=FIRE_RED)
    draw.ellipse([14, 22, 18, 26], fill=FIRE_RED)

    # Speed lines for quick browsing
    draw_speed_lines(draw)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_line_length_calculator_icon():
    """Create Line Length Calculator icon - Ruler with connected lines"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Power orange gradient background
    create_gradient_background(draw, ICON_SIZE, (200, 100, 0), (100, 50, 0))

    # Draw ruler/measurement symbol
    draw.rectangle([4, 12, 28, 20], fill=GOLD)
    for x in range(6, 28, 3):
        draw.line([(x, 13), (x, 16)], fill=(0, 0, 0), width=1)

    # Draw connected line segments above
    draw.line([(6, 8), (16, 8), (16, 4), (26, 4)], fill=(255, 255, 255), width=2)

    # Lightning accent for speed
    draw_lightning_bolt(draw, offset_x=-2, offset_y=-4, color=LIGHTNING_YELLOW)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def main():
    """Generate all BIMKraft icons"""
    # Create output directory
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    icons = {
        # Parameter Tools
        'parameter_pro.png': create_parameter_pro_icon(),
        'parameter_transfer_pro.png': create_parameter_transfer_icon(),

        # Workset Tools
        'workset_manager.png': create_workset_manager_icon(),

        # Quality Tools
        'warnings_browser_pro.png': create_warnings_browser_pro_icon(),

        # Measurement Tools
        'line_length_calculator.png': create_line_length_calculator_icon(),

        # Family Tools
        'family_renamer.png': create_family_renamer_icon(),
    }

    print("=" * 50)
    print("        BIM KRAFT ICON GENERATOR")
    print("        Power & Speed Themed Icons")
    print("=" * 50)
    print()
    print(f"Generating {len(icons)} icons...")
    print()

    for filename, img in icons.items():
        filepath = os.path.join(OUTPUT_DIR, filename)
        img.save(filepath, 'PNG')
        print(f"* {filename:<35} -> {filepath}")

    print()
    print("=" * 60)
    print("SUCCESS - All icons generated!")
    print("=" * 60)
    print()
    print("Icon Specifications:")
    print(f"  - Size: {ICON_SIZE[0]}x{ICON_SIZE[1]} pixels (Revit ribbon standard)")
    print(f"  - Format: PNG with RGBA transparency")
    print(f"  - Theme: Electric Blue + Lightning Yellow (BIM Power)")
    print(f"  - Effects: Radial gradients, lightning bolts, speed lines")
    print()
    print("Brand Colors:")
    print(f"  - Electric Blue:    RGB(0, 150, 255)")
    print(f"  - Lightning Yellow: RGB(255, 255, 0)")
    print(f"  - Power Orange:     RGB(255, 100, 0)")
    print(f"  - Steel Gray:       RGB(100, 120, 140)")
    print(f"  - Gold:             RGB(255, 215, 0)")
    print()
    print("Integration Instructions:")
    print("  1. Copy icons to: bimkraft-src/Resources/Icons/")
    print("  2. Update BIMKraftRibbonApplication.cs to reference icons")
    print("  3. Set icons as embedded resources in BIMKraft.csproj")
    print()
    print("See DESIGN.md for complete brand guidelines.")
    print()


if __name__ == "__main__":
    main()
