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
    """Create Parameter Pro icon - Gear with lightning"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background
    create_gradient_background(draw, ICON_SIZE, (50, 100, 200), (25, 50, 100))

    # Draw stylized 'P' with lightning
    draw.rectangle([8, 8, 12, 24], fill=GOLD)
    draw.ellipse([8, 8, 18, 16], fill=GOLD)

    # Small lightning bolt
    draw_lightning_bolt(draw, offset_x=6, offset_y=4, color=LIGHTNING_YELLOW)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_workset_tools_icon():
    """Create Workset Tools icon - Layers with power effect"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background
    create_gradient_background(draw, ICON_SIZE, STEEL_GRAY, (50, 60, 70))

    # Draw stacked layers
    for i, y in enumerate([8, 14, 20]):
        alpha = 255 - i * 40
        draw.rectangle([6, y, 26, y+3], fill=(255, 215, 0, alpha))

    # Lightning accent
    draw_lightning_bolt(draw, offset_x=-4, offset_y=0, color=LIGHTNING_YELLOW)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_quality_tools_icon():
    """Create Quality Tools icon - Check mark with power"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background
    create_gradient_background(draw, ICON_SIZE, ENERGY_GREEN, (25, 100, 25))

    # Draw check mark
    check_points = [(8, 16), (14, 22), (24, 10)]
    draw.line(check_points, fill=LIGHTNING_YELLOW, width=4, joint='curve')

    # Speed lines
    draw_speed_lines(draw)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def create_measurement_tools_icon():
    """Create Measurement Tools icon - Ruler with lightning"""
    img = Image.new('RGBA', ICON_SIZE, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Gradient background
    create_gradient_background(draw, ICON_SIZE, (200, 100, 0), (100, 50, 0))

    # Draw ruler
    draw.rectangle([4, 12, 28, 20], fill=GOLD)
    for x in range(6, 28, 3):
        draw.line([(x, 13), (x, 15)], fill=(0, 0, 0), width=1)

    # Lightning accent
    draw_lightning_bolt(draw, offset_x=-2, offset_y=-2, color=LIGHTNING_YELLOW)

    # Glossy border
    draw_glossy_border(draw, ICON_SIZE)

    return img


def main():
    """Generate all BIMKraft icons"""
    # Create output directory
    os.makedirs(OUTPUT_DIR, exist_ok=True)

    icons = {
        'family_renamer.png': create_family_renamer_icon(),
        'warnings_browser.png': create_warnings_browser_icon(),
        'parameter_pro.png': create_parameter_pro_icon(),
        'workset_tools.png': create_workset_tools_icon(),
        'quality_tools.png': create_quality_tools_icon(),
        'measurement_tools.png': create_measurement_tools_icon(),
    }

    print("BIM Kraft Icon Generator")
    print("=" * 50)
    print(f"Generating {len(icons)} power & speed themed icons...")
    print()

    for filename, img in icons.items():
        filepath = os.path.join(OUTPUT_DIR, filename)
        img.save(filepath, 'PNG')
        print(f"✓ Created: {filepath}")

    print()
    print("=" * 50)
    print(f"All icons generated successfully in '{OUTPUT_DIR}' folder!")
    print()
    print("Icon Details:")
    print(f"  - Size: {ICON_SIZE[0]}x{ICON_SIZE[1]} pixels")
    print(f"  - Format: PNG with transparency")
    print(f"  - Theme: Power & Speed (lightning, gradients, dynamic effects)")
    print()
    print("To use these icons in Revit, copy them to:")
    print("  - For PyRevit: [tool].pushbutton/icon.png")
    print("  - For C# Add-in: Update button code with icon paths")


if __name__ == "__main__":
    main()
