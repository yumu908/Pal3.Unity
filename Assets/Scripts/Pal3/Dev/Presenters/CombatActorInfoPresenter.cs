// ---------------------------------------------------------------------------------------------
//  Copyright (c) 2021-2023, Jiaqi Liu. All rights reserved.
//  See LICENSE file in the project root for license information.
// ---------------------------------------------------------------------------------------------

namespace Pal3.Dev.Presenters
{
    using Core.DataReader.Gdb;
    using UnityEngine;

    /// <summary>
    /// CombatActorInfo holder component to present ScnObjectInfo in the Unity inspector.
    /// </summary>
    public sealed class CombatActorInfoPresenter : MonoBehaviour
    {
        [SerializeField] public CombatActorInfo combatActorInfo;
    }
}