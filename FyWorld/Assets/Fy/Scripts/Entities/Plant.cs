/*
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
| FyWorld - A top down simulation game in a fantasy medieval world.    |
|                                                                      |
|    :copyright: © 2019 Florian Gasquez.                               |
|    :license: GPLv3, see LICENSE for more details.                    |
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
*/
using System.Collections.Generic;
using UnityEngine;
using Fy.Definitions;
using Fy.Visuals;
using Fy.Helpers;
using Fy.World;

namespace Fy.Entities {
	// Plant
	public class Plant : Tilable {
		/* Color for the leafs */
		private Color _leafColor;

		/* Color for the wood (only used for trees) */
		private Color _woodColor;

		/* Plant lifetime in ticks */
		private float _lifetime;

		/* Ticks per state */
		private float _ticksPerState;

		/* Size per state, at the final state the size should be 1 */
		private float _sizePerState;

		/* Current plant state */
		private int _currentState;

		/* Plant current grow state */
		public int state { get { return this._currentState; } }

		public Plant(Vector2Int position, TilableDef def, bool randomGrow = false) {
			this.addGraphics = new Dictionary<string, GraphicInstance>();
			this.position = position;
			this.def = def;
			this._lifetime = this.def.plantDef.lifetime*Settings.TICKS_PER_DAY;
			this._ticksPerState = this._lifetime/this.def.plantDef.states;
			this._sizePerState = 1f/this.def.plantDef.states;

			if (randomGrow) {
				this.ticks = Random.Range(0, (int)(this._lifetime-this._ticksPerState));
			} else {
				this.ticks = 0;
				this._currentState = 1;
			}
			this.GetState();
			this.UpdateGraphics();
			Loki.tick.toAdd.Enqueue(this.Update);
		}

		public override void UpdateGraphics() {
			if (this.def.type == TilableType.Grass) { // If we are a grass
				this._leafColor = Defs.colorPallets["cols_leafsGreen"].GetRandom();
				this.mainGraphic = GraphicInstance.GetNew(def.graphics, this._leafColor	);
			} else if (this.def.type == TilableType.Tree) { // If we are groot

				this._leafColor = Defs.colorPallets["cols_leafsGreen"].GetRandom();
				this._woodColor = Defs.colorPallets["cols_wood"].colors[0];
				this.mainGraphic = GraphicInstance.GetNew(
					def.graphics, 
					this._woodColor, 
					Res.textures[this.def.graphics.textureName+"_base"],
					1
				);

				if (this.addGraphics.ContainsKey("leafs")) {
					this.addGraphics["leafs"] = GraphicInstance.GetNew(
						def.graphics,
						this._leafColor,
						Res.textures[this.def.graphics.textureName+"_leafs"],
						2
					);
				} else {
					this.addGraphics.Add("leafs", 
						GraphicInstance.GetNew(
							def.graphics,
							this._leafColor,
							Res.textures[this.def.graphics.textureName+"_leafs"],
							2
						)
					);
				}
			} else { // Standard graphics no coloring or override.
				this.mainGraphic = GraphicInstance.GetNew(def.graphics);
			}
		}

		// Calculate the current plant state.
		private void GetState() {
			int state = Mathf.CeilToInt(this.ticks/this._ticksPerState);
			if (state > this.def.plantDef.states) {
				state = this.def.plantDef.states;
			} 

			if (state != this._currentState) {
				this._currentState = state;
				this.scale = new Vector3(
					this._currentState*this._sizePerState, 
					this._currentState*this._sizePerState, 
					1
				);
				if (this.bucket != null) {
					this.bucket.rebuildMatrices = true;
				}
			}
		}

		public override void AddOrder(MenuOrderDef def) {
			base.AddOrder(def);
			WorldUtils.cutOrdered.Add((Tilable)this);
		}

		public override void ClearOrder() {
			if (WorldUtils.cutOrdered.Contains(this)) {
				WorldUtils.cutOrdered.Remove(this);
			}
			base.ClearOrder();
		}

		// Called each tick
		public void Update() {
			this.ticks++;
			this.GetState();
			if (this.ticks >= this._lifetime) {
				// Also we want to try to reproduce here.
				this.Destroy();
			}
		}

		// Destroy our plant. R.I.P
		public override void Destroy() {
			Loki.tick.toDel.Enqueue(this.Update);
			base.Destroy();
		}

		public void Cut() {
			if (WorldUtils.cutOrdered.Contains(this)) {
				WorldUtils.cutOrdered.Remove(this);
			}

			int qtyLoot = (int)(Defs.stackables["logs"].maxStack/((this.def.plantDef.states+1)-this._currentState));

			if (this.def.type == TilableType.Tree) {
				Loki.map.Spawn(this.position, new Stackable(
					this.position,
					Defs.stackables["logs"],
					qtyLoot
				));
			}
			this.Destroy();
		}
	}
}