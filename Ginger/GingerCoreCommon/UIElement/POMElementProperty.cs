﻿using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common.Enums;
using static Amdocs.Ginger.Common.UIElement.ElementInfo;

namespace Amdocs.Ginger.Common.UIElement
{
    public class POMElementProperty : ControlProperty
    {


        public eDeltaStatus DeltaElementProperty { get; set; }

        public eImageType DeltaStatusIcon
        {
            get
            {
                switch (DeltaElementProperty)
                {
                    case ElementInfo.eDeltaStatus.Deleted:
                        return eImageType.Deleted;
                    case ElementInfo.eDeltaStatus.Modified:
                        return eImageType.Modified;
                    case ElementInfo.eDeltaStatus.New:
                        return eImageType.Added;
                    case ElementInfo.eDeltaStatus.Unchanged:
                    default:
                        return eImageType.UnModified;
                }
            }
        }

        public string DeltaExtraDetails { get; set; }

        public string UpdatedValue { get; set; }

        public bool IsNotEqual
        {
            get
            {
                if (DeltaElementProperty == eDeltaStatus.Unchanged)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }

    }
}
