﻿using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using GingerCore.Variables;

namespace Amdocs.Ginger.Common
{
    public interface IBusinessFlow
    {
        eRunStatus RunStatus { get; set; }

        bool Active { get; set; }
        Guid Guid { get; set; }
        Guid InstanceGuid { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string RunDescription { get; set; }
        string Environment { get; set; }
        Single? ElapsedSecs { get; set; }
        DateTime StartTimeStamp { get; set; }
        DateTime EndTimeStamp { get; set; }
        ObservableList<IActivity> Activities { get; set; }
        ObservableList<IFlowControl> BFFlowControls { get; set; }

        ObservableList<VariableBase> Variables { get; set; }
        IActivity CurrentActivity { get; set; }
        string ExecutionLogFolder { get; set; }
        double? Elapsed { get; set; }
        object Mandatory { get; set; }
        string ExecutionFullLogFolder { get; set; }

        ObservableList<VariableBase> GetVariables();

        VariableBase GetHierarchyVariableByName(string varName, bool considerLinkedVar = true);
        ObservableList<VariableBase> GetSolutionVariables();
        string GetPublishStatusString();
        int GetValidationsCount();
        int GetActionsCount();
        ObservableList<VariableBase> GetBFandActivitiesVariabeles(bool includeParentDetails, bool includeOnlySetAsInputValue = false, bool includeOnlySetAsOutputValue = false);
        IBusinessFlow CreateCopy(bool v);
        void Reset();
        void AttachActivitiesGroupsAndActivities();
    }
}
