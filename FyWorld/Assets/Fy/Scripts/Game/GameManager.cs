﻿/*
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
| FyWorld - A top down simulation game in a fantasy medieval world.    |
|                                                                      |
|    :copyright: © 2019 Florian Gasquez.                               |
|    :license: GPLv3, see LICENSE for more details.                    |
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fy.Definitions;
using Fy.World;
using Fy.Entities;
using Fy.Helpers;
using Fy.Controllers;
using Fy.Characters;
using Fy.UI;

namespace Fy {
	// Manage the game. (yep).
	public class GameManager : MonoBehaviour
	{
		/* Map */
		public CameraController cameraController;
		public StackableLabelController stackableLabelController;
		public Tick tick;
		public Map map;
		public bool DrawGizmosTiles = false;
		public bool DrawNoiseMap = false;
		public bool DrawRecipes = false;
		public bool DrawBuckets = false;
		public bool DrawAStar = false;
		public bool DrawFertility = false;
		public bool DrawPaths = false;
		public bool DrawReserved = false;
		public bool ready { get { return this._ready; } }
		public Vector2Int mapSize;

		/* Are we ready ? */
		private bool _ready;

		/// Load defs
		void Awake() {
			this._ready = false;
			this.cameraController = this.GetComponent<CameraController>();
			this.stackableLabelController = this.GetComponentInChildren<StackableLabelController>();
			Loki.LoadStatics();
			Loki.NewGame(this);
		}

		void TestStuff() {
			Debug.Log(this.map);
			/// TEST STUFF

			foreach (Vector2Int position in new RectI(new Vector2Int(10, 10), 5, 5)) {
				if (this.map[position].blockStackable == false) {
					this.map.Spawn(position, new Stackable(
						position,
						Defs.stackables["logs"],
						Random.Range(1, Defs.stackables["logs"].maxStack)
					));
				}
			}

			///// TEST WALLS
			int y = 22;
			for (int x = 10; x < 28; x++) {
				Loki.map.Spawn(new Vector2Int(x, y), new Building(
					new Vector2Int(x, y),
					Defs.buildings["wood_wall"]
				));
			}
			y = 30;
			for (int x = 10; x < 28; x++) {
				Loki.map.Spawn(new Vector2Int(x, y), new Building(
					new Vector2Int(x, y),
					Defs.buildings["wood_wall"]
				));
			}
			Loki.map.UpdateConnectedBuildings();
			///// TEST WALLS	

			/*for (int i = 0; i < 5; i++) {
				this.map.SpawnCharacter(new Animal(new Vector2Int(15,15), Defs.animals["chicken"]));
			}*/
			GrowArea area = new GrowArea(Defs.plants["carrot"]);
			area.Add(new RectI(new Vector2Int(15,15), 6, 6));

			StockArea stockarea = new StockArea(Defs.empty);
			stockarea.Add(new RectI(new Vector2Int(5,5), 6, 6));

			for (int i = 0; i < 5; i++) {
				this.map.SpawnCharacter(new Human(new Vector2Int(10,10), Defs.animals["human"]));
			}
			//Fy.Characters.AI.TargetList.GetRandomTargetInRange(new Vector2Int(10, 10));
			//new WindowBuildMenu();
		}


		/// Generating the map, spawning things.
		void Start() {
			this.tick = new Tick();
			this.map = new Map(this.mapSize.x, this.mapSize.y);
			this.map.TempMapGen();
			this.map.BuildAllMeshes();

			this.TestStuff();
	
			this.StartCoroutine(this.TickLoop());
			this._ready = true;
		}

		// Draw the regions
		void Update() {
			if (this._ready) {
				this.map.DrawTilables();
				this.map.DrawCharacters();
			}
		}

		void LateUpdate() {
			if (this._ready) {
				this.map.CheckAllMatrices();
			}
		}

		IEnumerator TickLoop() {
			for(;;) {
				yield return new WaitForSeconds(.01f/this.tick.speed);
				this.tick.DoTick();
			}
		}

		// WARNING WARNING : Clean this shit.
		void OnDrawGizmos() {
			if (this._ready && Settings.DEBUG) {
				if (this.DrawReserved) {
					DebugRenderer.DrawReserved();
				}
				if (this.DrawPaths) {
					foreach (BaseCharacter character in this.map.characters) {
						DebugRenderer.DrawCurrentPath(character.movement);
					}
				}
				if (this.DrawRecipes) {
					DebugRenderer.DrawRecipes();
				}
				if (this.DrawAStar) {
					DebugRenderer.DrawAStar();
				}
				if (this.DrawBuckets) {
					DebugRenderer.DrawBuckets();
				}
				if (this.DrawGizmosTiles) {
					DebugRenderer.DrawTiles();
				}
				if (this.DrawNoiseMap) {
					DebugRenderer.DrawNoiseMap();
				}
				if (this.DrawFertility) {
					DebugRenderer.DrawFertility();
				}
			}
		}
	}
}