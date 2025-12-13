# PyRevit Extension Bundle Configuration
# This file configures the Enhanced Warnings Browser as a PyRevit extension

from pyrevit import script

__title__ = "Enhanced\nWarnings"
__doc__ = """Enhanced Warnings Browser with element highlighting and detailed information.

Features:
- View warnings with additional columns (Element IDs, Names, Levels, Views, Categories)
- Automatic element highlighting in red when selecting warnings
- Navigate to elements in their respective views
- Export enhanced HTML reports
- Non-modal window for seamless workflow

Developed by ICL Ingenieur Consult GmbH"""

__author__ = "ICL Ingenieur Consult"
__helpurl__ = "https://icl-ingenieure.de"
__context__ = ["model"]
