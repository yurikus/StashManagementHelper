# StashManagementHelper

An SPT plugin to improve stash management.

Features:
- Customizable stash sorting
  -  Create your own sorting strategy or use the default EFT sorting with extra features.
  -  Automatically fold items during sorting to save space.
  -  Merge stacks of the same item.
  -  Rotate items for best fit.
  -  Skip a set amount of first rows to leave empty place to dump your items.
  -  Sort items starting from the bottom.
    
![image](https://github.com/Markosz22/StashManagementHelper/assets/41615461/8aad2256-378c-4fd0-9f13-340247cda926)
    
- Configure your own sorting strategy:
  - Sort by container size
  - Sort by item size
  - Sort by item type - You can even configure the item type order (work in progress)
 
![image](https://github.com/Markosz22/StashManagementHelper/assets/41615461/521ecce7-ceda-45cc-8ab2-7539cb11b550)

How to use:

- Change the settings to however you like and press the sort button. By default it's set up for maximum space efficiency.
- (1) Options apply to both default and custom sorting
- After you sort for the first time, a customSortConfig.json will be created next to the dll and you can customize the order by changing the order of items in the list:
  - sortOrder configures in which order the different order types apply (if enabled).
  - itemTypeOrder configures "Sort by item type" in which order the different item types are sorted (if enabled).
