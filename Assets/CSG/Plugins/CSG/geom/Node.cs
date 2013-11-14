using UnityEngine;
using System.Collections.Generic;
//using System.Linq;

public class Node
{
	public Plane plane;
	public Node front;
	public Node back;
    public List<Polygon> polygons = new List<Polygon>(); // TODO: optimization: this could probably be an array
	
	public Node(List<Polygon> polygons) {
		if (polygons.Count > 0) {
			build(polygons);
		}
	}
	
	public Node() {
		//this.polygons = new List<Polygon>();
	}
	
	public Node clone() {
		Node node = new Node();
		if (this.plane != null) node.plane = this.plane.clone();
		if (this.front != null) node.front = this.front.clone();
		if (this.back != null) node.back = this.back.clone();
		foreach (Polygon p in this.polygons) {
			node.polygons.Add(p.clone());
		}
		return node;
	}
	
	/**
	 * Convert solid space to empty space and empty space to solid space.
	 */ 
	public void invert() {
		for (int i = 0; i < this.polygons.Count; i++) {
			this.polygons[i].flip();
		}
		this.plane.flip();
		if (this.front != null) this.front.invert();
		if (this.back != null) this.back.invert();
		Node temp = this.front;
		this.front = this.back;
		this.back = temp;
	}
	
	/**
	 * Recursively remove all polygons in `polygons` that are inside this BSP
	 * tree.
	 * @param polygons
	 */ 
	public List<Polygon> clipPolygons(List<Polygon> polys) {
		if (this.plane == null) return new List<Polygon>(polys);
		List<Polygon> front = new List<Polygon>();
		List<Polygon> back = new List<Polygon>();
		for (int i = 0; i < polys.Count; i++) {
			this.plane.splitPolygon(polys[i], ref front, ref back, ref front, ref back);
		}
		if (this.front != null) front = this.front.clipPolygons(front);
		if (this.back != null) {
			back = this.back.clipPolygons(back);
		} else { back.Clear(); }
		front.AddRange(back);
		return front;
	}
	
	/**
	 * Remove all polygons in this BSP tree that are inside the other BSP tree
	 *`bsp`.
	 * @param bsp
	 */ 
	public void clipTo(Node bsp) {
		this.polygons = bsp.clipPolygons(this.polygons);
		if (this.front != null) this.front.clipTo(bsp);
		if (this.back != null) this.back.clipTo(bsp);
	}
	
	/**
	 *  Return a list of all polygons in this BSP tree.
	 */ 
	public List<Polygon> allPolygons() {
		List<Polygon> polys = new List<Polygon>(this.polygons);
		if (this.front != null) polys.AddRange(this.front.allPolygons());
		if (this.back != null) polys.AddRange(this.back.allPolygons());
		return polys;
	}
	
	/**
	  * Build a BSP tree out of `polygons`. When called on an existing tree, the
	  * new polygons are filtered down to the bottom of the tree and become new
	  * nodes there. Each set of polygons is partitioned using the first polygon
	  * (no heuristic is used to pick a good split).
	  */
	public void build(List<Polygon> polys) {
		if (polys.Count == 0) return;
		if (this.plane == null) this.plane = polys[0].plane.clone();
		List<Polygon> front = new List<Polygon>(); 
		List<Polygon> back = new List<Polygon>(); 
		for (int i = 0; i < polys.Count; i++) {
			this.plane.splitPolygon(polys[i], ref this.polygons, ref this.polygons, ref front, ref back);
		}
		if (front.Count > 0) {
			if (this.front == null) this.front = new Node();
			this.front.build(front);
		}
		if (back.Count > 0) {
			if (this.back == null) this.back = new Node();
			this.back.build(back);
		}
	}
}