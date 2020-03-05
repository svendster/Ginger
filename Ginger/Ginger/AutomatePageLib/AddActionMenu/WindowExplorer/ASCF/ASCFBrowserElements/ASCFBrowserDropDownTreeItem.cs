#region License
/*
Copyright © 2014-2020 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using System.Windows.Controls;
using GingerCore.Actions;
using GingerCore.Actions.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.ASCF
{
    class ASCFBrowserDropDownTreeItem : ASCFBrowserElementTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            //TODO: put text icon
            string ImageFileName = "@DropDownList_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            // click link the most common
            ActASCFBrowserElement a2 = new ActASCFBrowserElement();
            a2.Description = "Click Drop Down" + ASCFBrowserElementInfo.Path;
            a2.LocateBy = eLocateBy.ByID;
            a2.LocateValue = ASCFBrowserElementInfo.Path;
            a2.ControlAction = ActASCFBrowserElement.eControlAction.Click;            
            list.Add(a2);
            return list;
        }
    }
}
