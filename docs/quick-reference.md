# BIMKraft Quick Reference

**Version 0.1.0** | **Print this page for quick access at your desk!**

---

## 🚀 Getting Started

1. Download from **bimkraft.de/download**
2. Run MSI installer
3. Restart Revit
4. Find **BIMKraft** ribbon tab

**Support**: support@bimkraft.de | **Docs**: bimkraft.de/documentation

---

## 🛠️ Tools Overview

| Tool | Purpose | Key Feature |
|------|---------|-------------|
| **Parameter Pro** | Manage project parameters | Batch edit, filter, create parameters |
| **Parameter Transfer Pro** | Copy parameter values | Map source → target parameters |
| **Workset Manager** | Rule-based workset assignment | Built-in presets, auto-create worksets |
| **Warnings Browser Pro** | Analyze project warnings | Filter, export, zoom to elements |
| **Line Length Calculator** | Calculate line lengths | Color-code, group connected lines |
| **Family Renamer** | Batch rename families | Prefix, suffix, find & replace |

---

## 📏 Parameter Pro - Quick Steps

1. Click **Parameter Pro** button
2. **Search** to filter parameters
3. **Add Parameter** → Choose project/shared → Fill details
4. **Edit** existing parameters
5. **Save Preset** for reuse

**Tip**: Use shared parameters for schedule data!

---

## 🔄 Parameter Transfer Pro - Quick Steps

1. Click **Parameter Transfer Pro**
2. **Select Source** elements (values to copy FROM)
3. **Select Target** elements (values to copy TO)
4. **Configure Mapping**: Source Param → Target Param
5. Click **Transfer**

**Tip**: Save mappings as presets for recurring workflows!

---

## 📂 Workset Manager - Quick Steps

### Load Built-in Preset:
1. Click **Workset Manager**
2. **Load Preset** → Select from 6 presets
3. Click **Process**

### Custom Workset:
1. **New Workset** → Enter name & description
2. **Add Rule** → Choose type (Category, Parameter, etc.)
3. Configure rule details
4. Click **Process Workset**

**Built-in Presets**:
- `BIMKraft_ALL_Raster-Ebenen` (Grids & Levels)
- `BIMKraft_ALL_Referenzen` (Links)
- `BIMKraft_ARC` (Architectural)
- `BIMKraft_TWP_Bewehrung` (Reinforcement)
- `BIMKraft_TWP_Rohbau` (Structural Shell)
- `BIMKraft_TWP_Stahl` (Steel)

---

## ⚠️ Warnings Browser Pro - Quick Steps

1. Click **Warnings Browser Pro**
2. **Filter** by severity/category/keyword
3. **Select warning** → **Zoom to Element**
4. Fix in Revit
5. **Export** to Excel/CSV for tracking

**Tip**: Group similar warnings to resolve in batches!

---

## 📐 Line Length Calculator - Quick Steps

### All Lines in View:
1. Open view with lines
2. Click **Line Length Calculator**
3. Uncheck "Use Selection"
4. Click **Calculate**

### Selected Lines:
1. **Select Lines** → Pick in Revit
2. Check "Use Selection"
3. Click **Calculate**

### Actions:
- **Apply Colors** → Visualize in Revit
- **Export Excel/CSV** → Quantity takeoff
- **Create Groups** → Make Revit groups
- **Save Preset** → Remember colors

---

## 🏷️ Family Renamer - Quick Steps

1. Click **Family Renamer**
2. **Filter** by category or search
3. **Select families** to rename
4. Choose operation:
   - **Add Prefix**: `"2025_" + name`
   - **Add Suffix**: `name + "_TypeA"`
   - **Find & Replace**: `"Old" → "New"`
   - **Number Sequence**: `001, 002, 003...`
5. Click **Preview**
6. Review changes
7. Click **Apply**

**Tip**: Always preview before applying!

---

## 💡 Best Practices

### ✅ DO:
- **Save before using tools** - Always save your work
- **Test on small dataset** - Before processing entire project
- **Use presets** - Save time on recurring tasks
- **Preview operations** - Check before applying
- **Sync in workshared** - Coordinate with team

### ❌ DON'T:
- **Process during collaboration** - Sync first
- **Delete system families** - Can break dependencies
- **Ignore warnings** - They indicate model health
- **Skip previews** - Always check before bulk operations
- **Use special characters** - In names (use _ or -)

---

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| **Ribbon not showing** | Reinstall, check Revit version 2023-2026 |
| **Tool won't open** | Close Revit, delete temp files, restart |
| **Slow performance** | Use filters, close views, process in sections |
| **License error** | Click "Manage License", re-enter key |
| **Export fails** | Close existing file, check permissions |

---

## 📞 Support & Resources

| Resource | Link/Contact |
|----------|--------------|
| **Email Support** | support@bimkraft.de (1-2 days) |
| **Documentation** | bimkraft.de/documentation |
| **Pricing** | bimkraft.de/pricing |
| **Download** | bimkraft.de/download |
| **GitHub** | github.com/manishpaulsimon/bimkraft |

---

## 🎯 Common Workflows

### Workflow 1: Standardize Project Parameters
1. Open **Parameter Pro**
2. Load saved preset for project type
3. Add any missing parameters
4. Save as new preset if modified

### Workflow 2: Organize by Worksets
1. Open **Workset Manager**
2. Load all 6 built-in presets
3. Click **Load All Presets**
4. Review and process

### Workflow 3: Quality Control Check
1. Open **Warnings Browser Pro**
2. Filter by "Errors" only
3. Export to Excel
4. Address top issues
5. Re-run to verify resolution

### Workflow 4: Detail Line Takeoff
1. Open detail view
2. **Line Length Calculator**
3. Calculate all lines
4. Add descriptions per group
5. Export to Excel
6. Use in cost estimate

### Workflow 5: Family Naming Standards
1. Open **Family Renamer**
2. Filter by category
3. Add prefix: "[Project]_"
4. Preview changes
5. Apply to standardize

---

## 🎓 Pro Tips

1. **Keyboard Shortcut**: Assign Revit shortcuts to favorite BIMKraft tools
2. **Preset Library**: Create project templates with saved presets
3. **Batch Export**: Export data regularly for project tracking
4. **Team Training**: Hold lunch-and-learn sessions on BIMKraft
5. **Feedback Loop**: Submit feature requests to shape future updates

---

## 📊 System Requirements

- **Revit**: 2023, 2024, 2025, 2026
- **OS**: Windows 10/11 (64-bit)
- **.NET**: 4.7.2+ or .NET 8.0
- **RAM**: 8GB min, 16GB recommended
- **Disk**: 50MB

---

## 🆘 Quick Fixes

**"Transaction Already Started"**
→ Cancel active commands, close dialogs, try again

**Slow Calculation**
→ Use filters, process categories separately

**Can't Export**
→ Close destination file, choose different location

**Missing Ribbon**
→ Check `%AppData%\Autodesk\Revit\Addins\[Version]\BIMKraft.addin`

---

**Print this page and keep it handy!**

*Last Updated: December 26, 2025*
