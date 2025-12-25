# -*- coding: utf-8 -*-
"""
Revit Python Script: Assign All Linked References to Workset (FIXED)
This script assigns all linked files (RVT, IFC, DWG, PDF, IMG, etc.) to a specified workset.
Fixed to use proper Revit API methods for workset assignment.
"""

import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
clr.AddReference('System')

from Autodesk.Revit.DB import *
from Autodesk.Revit.UI import *
from System.Collections.Generic import List
import sys

def assign_links_to_workset():
    """
    Main function to assign all linked references to the specified workset
    """
    # Get the current Revit document
    doc = __revit__.ActiveUIDocument.Document
    uidoc = __revit__.ActiveUIDocument
    
    # Check if the document is workshared
    if not doc.IsWorkshared:
        TaskDialog.Show("Error", "This document is not workshared. Worksets are only available in workshared models.")
        return
    
    # Workset name to assign links to
    target_workset_name = "ICL_ALL_Referenzen"
    
    try:
        # Get all worksets in the document
        worksets = FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset)
        target_workset = None
        
        # Find the target workset
        for workset in worksets:
            if workset.Name == target_workset_name:
                target_workset = workset
                break
        
        # If workset doesn't exist, create it
        if target_workset is None:
            t = Transaction(doc, "Create Workset")
            t.Start()
            try:
                target_workset = Workset.Create(doc, target_workset_name)
                t.Commit()
                print("Created new workset: {}".format(target_workset_name))
            except Exception as e:
                t.RollBack()
                TaskDialog.Show("Error", "Failed to create workset: {}".format(str(e)))
                return
        
        # Start transaction for assigning elements to workset
        transaction = Transaction(doc, "Assign Links to Workset")
        transaction.Start()
        
        try:
            assigned_count = 0
            error_count = 0
            processed_types = set()
            
            # Helper function to change element workset using WorksetTable
            def change_element_workset(element, target_workset_id):
                try:
                    # Use WorksetTable.SetWorkset for proper workset assignment
                    WorksetTable.SetWorkset(doc, element.Id, target_workset_id)
                    return True
                except Exception as e:
                    print("WorksetTable method failed for element {}: {}".format(element.Id, str(e)))
                    # Fallback: try setting parameter directly
                    try:
                        workset_param = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM)
                        if workset_param and not workset_param.IsReadOnly:
                            workset_param.Set(target_workset_id.IntegerValue)
                            return True
                    except Exception as e2:
                        print("Parameter method also failed for element {}: {}".format(element.Id, str(e2)))
                    return False
            
            # 1. Revit Links (RVT files)
            revit_links = FilteredElementCollector(doc).OfClass(RevitLinkInstance)
            for link in revit_links:
                try:
                    if link.WorksetId != target_workset.Id:
                        if change_element_workset(link, target_workset.Id):
                            assigned_count += 1
                            processed_types.add("Revit Link Instances")
                        else:
                            error_count += 1
                except Exception as e:
                    error_count += 1
                    print("Error processing Revit link instance {}: {}".format(link.Id, str(e)))
            
            # Also handle Revit Link Types
            revit_link_types = FilteredElementCollector(doc).OfClass(RevitLinkType)
            for link_type in revit_link_types:
                try:
                    if link_type.WorksetId != target_workset.Id:
                        if change_element_workset(link_type, target_workset.Id):
                            assigned_count += 1
                            processed_types.add("Revit Link Types")
                        else:
                            error_count += 1
                except Exception as e:
                    error_count += 1
                    print("Error processing Revit link type {}: {}".format(link_type.Id, str(e)))
            
            # 2. CAD Links (DWG, DGN, etc.) - Types
            cad_link_types = FilteredElementCollector(doc).OfClass(CADLinkType)
            for link_type in cad_link_types:
                try:
                    if link_type.WorksetId != target_workset.Id:
                        if change_element_workset(link_type, target_workset.Id):
                            assigned_count += 1
                            processed_types.add("CAD Link Types")
                        else:
                            error_count += 1
                except Exception as e:
                    error_count += 1
                    print("Error processing CAD link type {}: {}".format(link_type.Id, str(e)))
            
            # CAD Link instances
            cad_instances = FilteredElementCollector(doc).OfClass(ImportInstance)
            for instance in cad_instances:
                try:
                    if instance.WorksetId != target_workset.Id:
                        if change_element_workset(instance, target_workset.Id):
                            assigned_count += 1
                            processed_types.add("CAD Import Instances")
                        else:
                            error_count += 1
                except Exception as e:
                    error_count += 1
                    print("Error processing CAD instance {}: {}".format(instance.Id, str(e)))
            
            # 3. Image/Raster Links - Types
            try:
                image_types = FilteredElementCollector(doc).OfClass(ImageType)
                for img_type in image_types:
                    try:
                        if img_type.WorksetId != target_workset.Id:
                            if change_element_workset(img_type, target_workset.Id):
                                assigned_count += 1
                                processed_types.add("Image Types")
                            else:
                                error_count += 1
                    except Exception as e:
                        error_count += 1
                        print("Error processing image type {}: {}".format(img_type.Id, str(e)))
                
                # Image instances
                images = FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RasterImages)
                for img in images:
                    try:
                        if img.WorksetId != target_workset.Id:
                            if change_element_workset(img, target_workset.Id):
                                assigned_count += 1
                                processed_types.add("Image Instances")
                            else:
                                error_count += 1
                    except Exception as e:
                        error_count += 1
                        print("Error processing image instance {}: {}".format(img.Id, str(e)))
            except Exception as e:
                print("Error in image processing section: {}".format(str(e)))
            
            # 4. Point Clouds (if any)
            try:
                point_clouds = FilteredElementCollector(doc).OfClass(PointCloudInstance)
                for pc in point_clouds:
                    try:
                        if pc.WorksetId != target_workset.Id:
                            if change_element_workset(pc, target_workset.Id):
                                assigned_count += 1
                                processed_types.add("Point Cloud Instances")
                            else:
                                error_count += 1
                    except Exception as e:
                        error_count += 1
                        print("Error processing point cloud {}: {}".format(pc.Id, str(e)))
                
                # Point Cloud Types
                point_cloud_types = FilteredElementCollector(doc).OfClass(PointCloudType)
                for pc_type in point_cloud_types:
                    try:
                        if pc_type.WorksetId != target_workset.Id:
                            if change_element_workset(pc_type, target_workset.Id):
                                assigned_count += 1
                                processed_types.add("Point Cloud Types")
                            else:
                                error_count += 1
                    except Exception as e:
                        error_count += 1
                        print("Error processing point cloud type {}: {}".format(pc_type.Id, str(e)))
            except Exception as e:
                print("Error in point cloud processing section: {}".format(str(e)))
            
            # 5. Additional link types - Generic approach for any missed elements
            try:
                # Look for any elements that might be external references
                all_elements = FilteredElementCollector(doc).WhereElementIsNotElementType()
                for element in all_elements:
                    try:
                        # Check if element has external references (simplified check)
                        if hasattr(element, 'IsLinked') or 'link' in element.GetType().Name.lower():
                            if hasattr(element, 'WorksetId') and element.WorksetId != target_workset.Id:
                                if change_element_workset(element, target_workset.Id):
                                    assigned_count += 1
                                    processed_types.add("Other Linked Elements")
                                else:
                                    error_count += 1
                    except:
                        continue  # Skip elements that can't be processed
            except Exception as e:
                print("Error in additional elements section: {}".format(str(e)))
            
            # Commit the transaction
            transaction.Commit()
            
            # Show results
            message = "Assignment Complete!\n\n"
            message += "Workset: {}\n".format(target_workset_name)
            message += "Elements assigned: {}\n".format(assigned_count)
            if error_count > 0:
                message += "Errors encountered: {}\n".format(error_count)
            message += "\nProcessed types:\n"
            for ptype in sorted(processed_types):
                message += "- {}\n".format(ptype)
            
            if assigned_count == 0 and error_count == 0:
                message += "\nNo linked elements found or all elements were already assigned to this workset."
            elif error_count > 0:
                message += "\nNote: Some elements could not be moved due to API limitations or element constraints."
            
            TaskDialog.Show("Success", message)
            
        except Exception as e:
            transaction.RollBack()
            TaskDialog.Show("Error", "Failed to assign elements to workset: {}".format(str(e)))
            
    except Exception as e:
        TaskDialog.Show("Error", "Script error: {}".format(str(e)))

# Execute the function
if __name__ == "__main__":
    assign_links_to_workset()

# For button/macro usage, you can also call the function directly:
# assign_links_to_workset()