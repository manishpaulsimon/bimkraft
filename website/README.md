# BIM Kraft Website

Official landing page for BIM Kraft - Professional Revit Tools

## Tech Stack

- **Framework**: [Astro](https://astro.build/) v5.1
- **Styling**: [Tailwind CSS](https://tailwindcss.com/) v4.1
- **Deployment**: Static site ready for Hostinger or any static hosting

## Getting Started

### Prerequisites

- Node.js 18.x or later
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The site will be available at `http://localhost:4321`

### Build for Production

```bash
# Build the static site
npm run build

# Preview the production build
npm run preview
```

## Project Structure

```
website/
├── src/
│   ├── layouts/
│   │   └── Layout.astro        # Base layout with navigation and footer
│   ├── pages/
│   │   ├── index.astro         # Home page
│   │   ├── features.astro      # Features/Products page
│   │   ├── documentation.astro # Documentation page
│   │   ├── download.astro      # Download/Installation page
│   │   └── contact.astro       # Contact page
│   └── styles/
│       └── global.css          # Global Tailwind styles
├── public/                     # Static assets
├── astro.config.mjs           # Astro configuration
└── package.json
```

## Pages

- **Home** (`/`) - Hero section, features overview, and benefits
- **Features** (`/features`) - Detailed feature descriptions for all tools
- **Documentation** (`/documentation`) - User guides, quick start, and FAQs
- **Download** (`/download`) - Download links and installation guide
- **Contact** (`/contact`) - Contact form and support information

## Commands

All commands are run from the root of the project, from a terminal:

| Command                   | Action                                           |
| :------------------------ | :----------------------------------------------- |
| `npm install`             | Installs dependencies                            |
| `npm run dev`             | Starts local dev server at `localhost:4321`      |
| `npm run build`           | Build your production site to `./dist/`          |
| `npm run preview`         | Preview your build locally, before deploying     |
| `npm run astro ...`       | Run CLI commands like `astro add`, `astro check` |
| `npm run astro -- --help` | Get help using the Astro CLI                     |

## Deployment

### Build Output

The `npm run build` command generates a static site in the `dist/` folder ready for deployment.

### Deploying to Hostinger

1. Build the site: `npm run build`
2. Upload the contents of `dist/` to your Hostinger public_html folder
3. Configure your domain to point to the uploaded files

### Alternative Deployments

The site can be deployed to any static hosting service:
- Netlify
- Vercel
- GitHub Pages
- Cloudflare Pages

## Development

### Adding New Pages

Create a new `.astro` file in `src/pages/`. The file name will become the URL path.

### Modifying Styles

Edit `src/styles/global.css` for global styles or use Tailwind utility classes directly in components.

### Updating Navigation

Edit the navigation links in `src/layouts/Layout.astro`.

## Features

- 📱 Responsive design for all devices
- ⚡ Lightning-fast performance with Astro
- 🎨 Modern UI with Tailwind CSS
- 📖 SEO-friendly static site generation
- ♿ Accessible markup

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

See the main BIM Kraft repository for license information.

## Support

- GitHub: [BIM Kraft Repository](https://github.com/manishpaulsimon/bimkraft)
- Issues: [Report a Bug](https://github.com/manishpaulsimon/bimkraft/issues)

## Learn More

- [Astro Documentation](https://docs.astro.build)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
