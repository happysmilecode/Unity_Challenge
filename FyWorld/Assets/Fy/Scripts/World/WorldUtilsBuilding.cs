/*
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
| FyWorld - A top down simulation game in a fantasy medieval world.    |
|                                                                      |
|    :copyright: © 2019 Florian Gasquez.                               |
|    :license: GPLv3, see LICENSE for more details.                    |
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
*/
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fy.Entities;
using Fy.Helpers;
using Fy.Definitions;
using Fy.Characters.AI;
using Fy.Characters;

namespace Fy.World {
	public static partial class WorldUtils {
		public static List<Recipe> recipes = new List<Recipe>();

		public static bool HaulRecipeNeeded() {
			return (WorldUtils.recipes.Count > 0);
		}

		public static TargetList RecipesToComplete(int radius, BaseCharacter character) {
			int capacity = character.inventory.max;
			int currentNeeds = 0;
			Recipe first = null;
			TilableDef need = null;
			List<Recipe> toHaul = new List<Recipe>();
			TargetList targets = null;

			foreach (Recipe _recipe in WorldUtils.recipes) {
				if (!Loki.map[_recipe.position].reserved && !_recipe.finished && _recipe.canBeComplete) {
					if (currentNeeds >= capacity) {
						currentNeeds = capacity;
						break;
					}

					if (first == null) {
						first = _recipe;
						need = first.FirstNeed();
						currentNeeds += first.needs[need].free;
						capacity = (capacity > WorldUtils.stackablesCount[need]) ? WorldUtils.stackablesCount[need] : capacity; 
					} else {
						if (Utils.Distance(first.position, _recipe.position) <= radius) {
							toHaul.Add(_recipe);
							currentNeeds +=  _recipe.needs[need].free;
						}
					}
				}
			}

			if (first == null || need == null || toHaul.Count == 0) {
				return null;
			}

			int stackFound = 0;
			if (WorldUtils.stackables.ContainsKey(need)) { // WorldUtils.stackables.ContainsKey(need)
				foreach (Stackable stack in WorldUtils.stackables[need]) {
				//foreach (Stackable stack in Loki.map.grids[Layer.Stackable].GetTilables()) {
					if (stackFound >= currentNeeds) {
						break;
					}
					if (!Loki.map[stack.position].reserved && stack.inventory.count != 0 && stack.inventory.def == need) {
						stackFound += stack.inventory.count;
						if (targets == null) {
							targets = new TargetList((Tilable)stack);
						} else {
							targets.Enqueue((Tilable)stack);
						}
					}
				}
			} else {
				return null;
			}

			if (targets != null) {
				foreach (Recipe _recipe in toHaul) {
					targets.Enqueue((Tilable)_recipe.building);
				}
			}

			return targets;
		}
	}
}
/*
		public static TargetList RecipeInRadius(int max = 25) {
			Recipe recipe = null;
			foreach (Recipe _recipe in WorldUtils.recipes) {
				if (!Loki.map[_recipe.position].reserved && !_recipe.finished && _recipe.canBeComplete) {
					recipe = _recipe;
					break;
				}
			}

			if (recipe != null) {
				TilableDef need = null;
				foreach (Inventory inv in recipe.needs.Values) {
					if (!inv.full) {
						need = inv.def;
						if (WorldUtils.stackables.ContainsKey(need)) {
							break;
						} else {
							need = null;
						}
					}
				}
				
				if (need != null) {
					List<Recipe> recipes = new List<Recipe>();
					int current = recipe.needs[need].free;
					int onMap = WorldUtils.stackablesCount[need];
					max = (max > onMap) ? onMap : max; 
					recipes.Add(recipe);

					HashSet<Tilable> tilablesInRadius = new HashSet<Tilable>();
					Tilable.InRadius(Layer.Building, 20, recipe.building, recipe.building, ref tilablesInRadius);
					foreach (Tilable tilable in tilablesInRadius) {
						if (current >= max) {
							break;
						} 

						if (tilable != null) {
							Building building = (Building)tilable;
							if (
								!Loki.map[tilable.position].reserved && !building.recipe.finished &&
								building.recipe.needs.ContainsKey(need) && !building.recipe.needs[need].full
							) {
								recipes.Add(building.recipe);
								current += building.recipe.needs[need].free;
							}
						}
					}

					TargetList targets = null;

					int stackFound = 0;
					if (WorldUtils.stackables.ContainsKey(need)) {
						foreach (Stackable stack in WorldUtils.stackables[need]) {
							if (stackFound >= max) {
								break;
							}
							if (!Loki.map[stack.position].reserved && stack.inventory.count != 0) {
								stackFound += stack.inventory.count;
								if (targets == null) {
									targets = new TargetList((Tilable)stack);
								} else {
									targets.Enqueue((Tilable)stack);
								}
							}
						}
					}

					if (targets == null) {
						return null;
					}

					foreach (Recipe _recipe in recipes) {
						targets.Enqueue((Tilable)recipe.building);
					}

					return targets;
				}
			}

			return null;
		}
	}
	*/
