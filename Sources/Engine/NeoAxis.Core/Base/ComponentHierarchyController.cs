// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

namespace NeoAxis
{
	/// <summary>
	/// The class for managing the component hierarchy.
	/// </summary>
	public class ComponentHierarchyController
	{
		//!!!!threading

		internal Component rootComponent;
		internal Resource.Instance createdByResource;

		internal ESet<Component> objectsDeletionQueue = new ESet<Component>();
		bool hierarchyEnabled;
		//!!!!
		//bool hierarchyVisible;

		object lockObjectHierarchy = new object();

		//Simulation
		double simulationTime = -1;
		//!!!!
		//bool simulationEnabled;
		//bool systemPauseOfSimulationEnabled;
		//internal SimulationTypes simulationType;
		//internal SimulationStatuses simulationStatus = SimulationStatuses.StillNotSimulated;

		//

		public ComponentHierarchyController()
		{
		}

		public Component RootComponent
		{
			get { return rootComponent; }
		}

		public Resource.Instance CreatedByResource
		{
			get { return createdByResource; }
		}

		public bool HierarchyEnabled
		{
			get { return hierarchyEnabled; }
			set
			{
				if( hierarchyEnabled == value )
					return;
				hierarchyEnabled = value;

				rootComponent._UpdateEnabledInHierarchy( false );
			}
		}

		//!!!!
		//public bool HierarchyVisible
		//{
		//	get { return hierarchyVisible; }
		//	set
		//	{
		//		if( hierarchyVisible == value )
		//			return;
		//		hierarchyVisible = value;

		//		rootComponent._UpdateVisibleInHierarchy( false );
		//	}
		//}

		void ProcessObjectsDeletionQueue()
		{
			while( objectsDeletionQueue.Count != 0 )
			{
				var e = objectsDeletionQueue.GetEnumerator();
				e.MoveNext();
				Component c = e.Current;

				if( c.Parent != null )
					c.RemoveFromParent( false );
				else
					objectsDeletionQueue.Remove( c );
			}
		}

		//!!!!!!��������
		//!!!!��� ��� ��������?
		public void ProcessDelayedOperations()
		{
			ProcessObjectsDeletionQueue();
		}

		public object LockObjectHierarchy
		{
			get { return lockObjectHierarchy; }
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		///// <summary>
		///// Gets the current time of simulation.
		///// </summary>
		public double SimulationTime
		{
			get { return simulationTime; }
		}

		///// <summary>
		///// To reset current simulation time.
		///// </summary>
		///// <remarks>
		///// This method need call, after there was a long loading.
		///// This method is called, that the timer did not try to catch up with lagged behind time.
		///// </remarks>
		public void ResetSimulationTime()
		{
			simulationTime = -1;
			//simulationTickTime = EngineApp.Instance.EngineTime;

			//!!!!����
			//lastRenderTime = simulationTickTime;
			//lastRenderTimeStep = 0;
		}

		//!!!!MapObject
		//public static double SimulationTickDelta
		//{
		//	get { return simulationTickDelta; }
		//}

		//!!!!
		////!!!!��� ������������ ��� ���������� �������?
		//public delegate void SimulationTickDelegate( Component_Map map );//!!!!, float simulationTickDelta );
		//public event SimulationTickDelegate SimulationTick;

		//void SimulationStep()
		//{
		////send WorldTick message
		//if( SimulationType_IsServer() )
		//{
		//	//!!!!����
		//	//networkTickCounter++;
		//	//networkingInterface.SendWorldTickMessage();
		//}

		////physics world tick
		//if( SimulationType != SimulationTypes.ClientOnly )
		//{
		//	//physicsPerformanceCounter.Start();
		//	physicsScene.Simulate( SimulationTickDelta );
		//	//physicsPerformanceCounter.End();
		//}

		////timer ticks
		////entitySystemPerformanceCounter.Start();

		////MapObjects.PerformSimulationTick( simulationTickExecutedTime, SimulationType == SimulationTypes.ClientOnly );
		//{
		//	SimulationTick?.Invoke( this );

		//	//MapObject[] array = new MapObject[ objectsSubscribedToTicks.Count ];
		//	//objectsSubscribedToTicks.Keys.CopyTo( array, 0 );

		//	//foreach( MapObject obj in array )
		//	//{
		//	//	if( !obj.IsMustDestroy && obj.CreateTime != simulationTickTime )
		//	//	{
		//	//		//!!!!slowly. ����� ����� ESet ���������
		//	//		if( objectsSubscribedToTicks.ContainsKey( obj ) )
		//	//		{
		//	//			if( !clientTick )
		//	//				obj.CallTick();
		//	//			else
		//	//				obj.Client_CallTick();
		//	//		}
		//	//	}
		//	//}
		//}

		////entitySystemPerformanceCounter.End();
		//}

		public void SimulateOneStep()
		{
			if( rootComponent != null && rootComponent.EnabledInHierarchy )
				rootComponent.PerformSimulationStep();
			ProcessDelayedOperations();
		}

		public void PerformSimulationSteps()
		{
			ProcessDelayedOperations();

			//!!!!����� ���� ��: ����� ����� ���� ��� ����������� �� ������� ����������. ����� ��� ������ ��� � ���� � ������� �������� �� ������ ���� EngineApp.Instance.Time.

			//!!!!����
			//simulationStatus = SimulationStatuses.AlreadySimulated;

			//!!!!����
			//if( SimulationType == SimulationTypes.ClientOnly )
			//	return;

			double time = EngineApp.EngineTime;

			//!!!!����
			//if( !simulationEnabled || systemPauseOfSimulationEnabled )
			//{
			//	simulationTickTime = time;
			//	return;
			//}

			//reset time
			if( simulationTime < 0 )
				simulationTime = time;

			//loop
			double delta = ProjectSettings.Get.SimulationStepsPerSecondInv;
			while( time > simulationTime + delta )
			{
				simulationTime += delta;
				SimulateOneStep();
			}
		}
	}
}