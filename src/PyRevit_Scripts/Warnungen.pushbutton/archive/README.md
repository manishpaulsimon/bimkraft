# Archive Folder

This folder contains archived/backup versions of the Warnings Browser tool.

## Version History

### Original Version (Pre-Optimization)
- **Issues**: Performance problems due to expensive view iteration for every element
- **Problem**: Called `GetElementViews()` which iterated through ALL views for EVERY element in EVERY warning
- **Impact**: Caused Revit to hang during loading in large projects

### Current Optimized Version
- **Optimizations Applied**:
  1. Views are now cached once at startup instead of being queried repeatedly
  2. Only returns one view per element (floor plan preferred, 3D view as fallback)
  3. Breaks early when suitable view is found
  4. Uses cached view lists instead of creating new FilteredElementCollector for each element

- **Performance Impact**: Significantly faster load times, especially in projects with many warnings and views
