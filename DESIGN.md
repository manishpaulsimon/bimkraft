# BIMKraft Design Documentation

## Brand Identity

**BIMKraft** translates to **BIM Power** in German, embodying the concepts of:
- **Power**: Robust, capable, industrial strength
- **Speed**: Fast, efficient, lightning-quick operations
- **Energy**: Dynamic, electric, high-performance

## Brand Colors

### Primary Colors

#### Electric Blue (`#0096FF` / RGB: 0, 150, 255)
- **Usage**: Primary brand color, headers, main UI elements
- **Symbolism**: Technology, reliability, speed
- **Application**: Tool backgrounds, primary buttons, brand logo

#### Lightning Yellow (`#FFFF00` / RGB: 255, 255, 0)
- **Usage**: Accent color, highlights, lightning bolt icons
- **Symbolism**: Energy, power, speed, instant action
- **Application**: Lightning bolts, speed indicators, highlights

#### Power Orange (`#FF6400` / RGB: 255, 100, 0)
- **Usage**: Warning states, quality tools, attention elements
- **Symbolism**: Energy, power, alertness
- **Application**: Warning tools, error states, important actions

### Secondary Colors

#### Steel Gray (`#64788C` / RGB: 100, 120, 140)
- **Usage**: Neutral elements, workset tools, structure
- **Symbolism**: Industrial strength, construction, foundation
- **Application**: Workset tools, neutral backgrounds, borders

#### Energy Green (`#32C832` / RGB: 50, 200, 50)
- **Usage**: Success states, quality indicators, completion
- **Symbolism**: Success, quality, validation
- **Application**: Quality tools, success messages, check marks

#### Gold (`#FFD700` / RGB: 255, 215, 0)
- **Usage**: Premium features, highlights, premium tools
- **Symbolism**: Excellence, precision, value
- **Application**: Premium tool accents, measurement tools

#### Fire Red (`#FF3200` / RGB: 255, 50, 0)
- **Usage**: Errors, critical warnings, delete actions
- **Symbolism**: Urgency, importance, caution
- **Application**: Error states, warning triangles, critical actions

## Design Principles

### Speed & Motion
- **Lightning Bolts**: Primary symbol of speed and power
- **Speed Lines**: Motion indicators showing quick action
- **Gradient Effects**: Dynamic, energetic transitions
- **Sharp Angles**: Suggesting speed and precision

### Power & Strength
- **Bold Shapes**: Strong, clear geometric forms
- **Radial Gradients**: Energy radiating from center
- **High Contrast**: Clear, powerful visual impact
- **Glossy Effects**: Modern, polished appearance

### Professional & Industrial
- **Clean Design**: No clutter, clear purpose
- **Consistent Sizing**: 32x32 pixels for ribbon icons
- **RGBA Format**: Transparency support for modern UI
- **Scalable Elements**: Clear at all sizes

## Tool-Specific Color Schemes

### Parameter Tools
- **Primary**: Electric Blue (`#0096FF`)
- **Accent**: Gold (`#FFD700`)
- **Effect**: Lightning Yellow
- **Theme**: Precision and power in parameter management

### Workset Tools
- **Primary**: Steel Gray (`#64788C`)
- **Accent**: Gold (`#FFD700`)
- **Effect**: Lightning Yellow
- **Theme**: Industrial strength layering and organization

### Quality Tools
- **Primary**: Energy Green (`#32C832`) / Power Orange (`#FF6400`)
- **Accent**: Lightning Yellow
- **Effect**: Speed lines
- **Theme**: Fast quality validation and warnings

### Measurement Tools
- **Primary**: Power Orange (`#FF6400`)
- **Accent**: Gold (`#FFD700`)
- **Effect**: Lightning Yellow
- **Theme**: Precise, powerful measurements

### Family Tools
- **Primary**: Electric Blue (`#0096FF`)
- **Accent**: Lightning Yellow
- **Effect**: Speed lines and lightning bolts
- **Theme**: Lightning-fast family operations

## Icon Design Guidelines

### Size & Format
- **Dimensions**: 32x32 pixels
- **Format**: PNG with RGBA transparency
- **Border**: 2-4px glossy border with highlight
- **Safe Area**: 2px margin from edges

### Visual Elements
1. **Background**: Radial gradient from light to dark
2. **Main Symbol**: Tool-specific icon or letter
3. **Accent**: Lightning bolt or speed lines
4. **Border**: Glossy effect with highlight

### Consistency Rules
- All icons use power/speed theme
- Lightning bolts appear in most tools
- Gradients radiate from center
- Glossy border for modern look
- Maximum 3-4 colors per icon

## Typography (Future Reference)

### Recommended Fonts
- **Headings**: Montserrat Bold / Bebas Neue (powerful, industrial)
- **Body**: Inter / Roboto (clean, modern, readable)
- **Code**: Fira Code / JetBrains Mono (technical)

### Text Style
- **All Caps for Power**: BIMKRAFT (logo, headers)
- **Title Case for Tools**: Parameter Pro, Workset Manager
- **Sentence case for descriptions**

## UI/UX Principles

### Speed-Focused Design
- Minimize clicks to complete tasks
- Keyboard shortcuts for power users
- Batch operations by default
- Preview before apply

### Power User Features
- Save/load presets
- Advanced filtering
- Bulk operations
- Export capabilities

### Clear Feedback
- Status messages
- Progress indicators
- Success/error states with appropriate colors
- Preview before destructive operations

## Application in Code

### Icon Usage
```csharp
// Set icon for ribbon button
buttonData.LargeImage = new BitmapImage(new Uri("pack://application:,,,/BIMKraft;component/Resources/Icons/parameter_pro.png"));
```

### Color References
```csharp
// WPF Color definitions
Color electricBlue = Color.FromRgb(0, 150, 255);
Color lightningYellow = Color.FromRgb(255, 255, 0);
Color powerOrange = Color.FromRgb(255, 100, 0);
```

## Version History

- **v1.0** (2025-12-25): Initial design documentation
  - Established brand colors
  - Defined design principles
  - Created tool-specific color schemes

## Future Considerations

### Potential Expansions
- Dark mode theme (inverted gradients)
- Accessibility variations (colorblind-friendly)
- Animated icons for splash screens
- SVG versions for web/documentation
- 16x16 small icon variants

### Brand Evolution
- Maintain core electric blue + lightning yellow
- Can introduce new accent colors for tool categories
- Keep power/speed symbolism central
- Ensure consistency across all tools

---

**BIMKraft - BIM Power**
*Fast. Powerful. Professional.*
