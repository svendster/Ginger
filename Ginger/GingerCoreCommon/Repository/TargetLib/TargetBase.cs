﻿#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Amdocs.Ginger.Common.Repository
{
    public class TargetBase : RepositoryItemBase
    {        
        public virtual string Name { get; }// impl in subclass

        public bool Selected { get; set; }
        

        ePlatformType mPlatform;
        [IsSerializedForLocalRepository]
        public ePlatformType Platform
        {
            get
            {
                return mPlatform;
            }
            set
            {
                if (mPlatform != value)
                {
                    mPlatform = value;
                    OnPropertyChanged(nameof(Platform));
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return Name;
            }
            set
            {
                // Do nothing
            }
        }

        // Save the last agent who executed on this Target
        [IsSerializedForLocalRepository]
        public string LastExecutingAgentName { get; set; }
    }
}
