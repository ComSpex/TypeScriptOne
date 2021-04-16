using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfColor = System.Windows.Media.Color;

namespace Raytracer {
	/// <summary>
	/// Vector
	/// </summary>
	public class Vector {
		public double x, y, z;
		public Vector(double x,double y,double z) {
			this.x=x;
			this.y=y;
			this.z=z;
		}
		public static Vector times(double k,Vector v) {
			return new Vector(k*v.x,k*v.y,k*v.z);
		}
		public static Vector minus(Vector v1,Vector v2) {
			System.Diagnostics.Debug.Assert(v1!=null);
			System.Diagnostics.Debug.Assert(v2!=null);
			return new Vector(v1.x-v2.x,v1.y-v2.y,v1.z-v2.z);
		}
		public static Vector plus(Vector v1,Vector v2) {
			return new Vector(v1.x+v2.x,v1.y+v2.y,v1.z+v2.z);
		}
		public static double dot(Vector v1,Vector v2) {
			return v1.x*v2.x+v1.y*v2.y+v1.z*v2.z;
		}
		public static double mag(Vector v) {
			return Math.Sqrt(v.x*v.x+v.y*v.y+v.z*v.z);
		}
		public static Vector norm(Vector v) {
			double _mag = Vector.mag(v);
			double _div = (_mag==0.0 ? double.PositiveInfinity : 1.0/_mag);
			return Vector.times(_div,v);
		}
		public static Vector cross(Vector v1,Vector v2) {
			return new Vector(
				v1.y*v2.z-v1.z*v2.y,
				v1.z*v2.x-v1.x*v2.z,
				v1.x*v2.y-v1.y*v2.x
			);
		}
	}
	/// <summary>
	/// Color
	/// </summary>
	public class Color {
		public double r, g, b;
		public Color(double r,double g,double b) {
			this.r=r;
			this.g=g;
			this.b=b;
		}
		public static Color scale(double k,Color v) {
			return new Color(k*v.r,k*v.g,k*v.b);
		}
		public static Color plus(Color v1,Color v2) {
			return new Color(v1.r+v2.r,v1.g+v2.g,v1.b+v2.b);
		}
		public static Color minus(Color v1,Color v2) {
			return new Color(v1.r-v2.r,v1.g-v2.g,v1.b-v2.b);
		}
		public static Color times(Color v1,Color v2) {
			return new Color(v1.r*v2.r,v1.g*v2.g,v1.b*v2.b);
		}
		public static Color white = new Color(1.0,1.0,1.0);
		public static Color grey = new Color(0.5,0.5,0.5);
		public static Color black = new Color(0,0,0);
		public static Color background = Color.black;
		public static Color defaultColor = Color.black;
		static double regalize(double d) {
			return d>1.0 ? 1.0 : d;
		}
		public static Color toDrawingColor(Color c) {
			return new Color(
				Math.Floor(regalize(c.r)*255.0),
				Math.Floor(regalize(c.g)*255.0),
				Math.Floor(regalize(c.b)*255.0)
			);
		}
	}
	/// <summary>
	/// Camera
	/// </summary>
	public class Camera {
		public Vector forward, right, up;
		public Camera(Vector pos,Vector lookAt) {
			this.pos=pos;
			Vector down = new Vector(0.0,-1.0,0.0);
			this.forward=Vector.norm(Vector.minus(lookAt,this.pos));
			this.right=Vector.times(1.5,Vector.norm(Vector.cross(this.forward,down)));
			this.up=Vector.times(1.5,Vector.norm(Vector.cross(this.forward,this.right)));
		}
		Vector _pos = new Vector(0,0,0);
		public Vector pos {
			get { return _pos; }
			set { _pos=value; }
		}
	}
	public interface IRay {
		Vector start { get; set; }
		Vector dir { get; set; }
	}
	public class Ray:IRay {
		Vector _start, _dir;
		public Ray(Vector start,Vector dir) {
			this._start=start;
			this._dir=dir;
		}
		public Vector start {
			get { return _start; }
			set { _start=value; }
		}
		public Vector dir {
			get { return _dir; }
			set { _dir=value; }
		}
	}
	public interface IIntersection {
		IThing thing { get; set; }
		IRay ray { get; set; }
		double dist { get; set; }
	}
	public class Intersection:IIntersection {
		private IThing _thing;
		private IRay _ray;
		private double _dist;
		public Intersection(IThing thing,IRay ray,double dist) {
			this.thing=thing;
			this.ray=ray;
			this.dist=dist;
		}
		public IThing thing {
			get { return _thing; }
			set { _thing=value; }
		}
		public IRay ray {
			get { return _ray; }
			set { _ray=value; }
		}
		public double dist {
			get { return _dist; }
			set { _dist=value; }
		}
	}
	public interface ISurface {
		Color diffuse(Vector pos);
		Color specular(Vector pos);
		double reflect(Vector pos);
		double roughness { get; set; }
	}
	public interface IThing {
		IIntersection intersect(IRay ray);
		Vector normal(Vector pos);
		ISurface surface { get; set; }
	}
	public interface ILight {
		Vector pos { get; set; }
		Color color { get; set; }
	}
	public class Light:ILight {
		Vector _pos;
		Color _color;
		public Light(Vector p,Color c) {
			_pos=p;
			_color=c;
		}
		public Vector pos {
			get { return _pos; }
			set { _pos=value; }
		}
		public Color color {
			get { return _color; }
			set { _color=value; }
		}
	}
	public interface IScene {
		IThing[] things { get; set; }
		ILight[] lights { get; set; }
		Camera camera { get; set; }
	}
	/// <summary>
	/// Sphere
	/// </summary>
	public class Sphere:IThing {
		public double radius2;
		public Vector center;
		public Sphere(Vector center,double radius,ISurface surface) {
			this.radius2=radius*radius;
			this.surface=surface;
			this.center=center;
		}
		public IIntersection intersect(IRay ray) {
			Vector eo = Vector.minus(this.center,ray.start);
			double v = Vector.dot(eo,ray.dir);
			double dist = 0.0;
			if(v>=0) {
				double disc = this.radius2-(Vector.dot(eo,eo)-v*v);
				if(disc>=0) {
					dist=v-Math.Sqrt(disc);
				}
			}
			if(dist==0) {
				return null;
			}
			return new Intersection(this,ray,dist);
		}
		public Vector normal(Vector pos) {
			return Vector.norm(Vector.minus(pos,this.center));
		}
		ISurface _surface;
		public ISurface surface {
			get { return _surface; }
			set { _surface=value; }
		}
	}
	/// <summary>
	/// Plane
	/// </summary>
	public class Plane:IThing {
		Vector norm;
		double offset;
		public Plane(Vector norm,double offset,ISurface surface) {
			this.norm=norm;
			this.offset=offset;
			this.surface=surface;
		}
		public IIntersection intersect(IRay ray) {
			double denom = Vector.dot(norm,ray.dir);
			if(denom>0) {
				return null;
			}
			double dist = (Vector.dot(norm,ray.start)+offset)/(-denom);
			return new Intersection(this,ray,dist);
		}
		public Vector normal(Vector pos) {
			return norm;
		}
		ISurface _surface;
		public ISurface surface {
			get { return _surface; }
			set { _surface=value; }
		}
	}
	/// <summary>
	/// Surfaces
	/// </summary>
	public static class Surfaces {
		public class Shiny:ISurface {
			public Color diffuse(Vector pos) {
				return Color.white;
			}
			public Color specular(Vector pos) {
				return Color.grey;
			}
			public double reflect(Vector pos) {
				return 0.7;
			}
			public double roughness {
				get { return 250; }
				set {
					throw new NotImplementedException();
				}
			}
		}
		public class Checkerboard:ISurface {
			public Color diffuse(Vector pos) {
				if((Math.Floor(pos.z)+Math.Floor(pos.x))%2!=0) {
					return Color.white;
				}
				return Color.black;
			}
			public Color specular(Vector pos) {
				return Color.white;
			}
			public double reflect(Vector pos) {
				if((Math.Floor(pos.z)+Math.Floor(pos.x))%2!=0) {
					return 0.1;
				}
				return 0.7;
			}
			public double roughness {
				get { return 150; }
				set {
					throw new NotImplementedException();
				}
			}
		}
		public static ISurface shiny = new Shiny();
		public static ISurface checkerboard = new Checkerboard();
	}
	/// <summary>
	/// RayTracer
	/// </summary>
	public class RayTracer {
		double maxDepth = 5.0;
		IIntersection intersections(IRay ray,IScene scene) {
			double closest = double.PositiveInfinity;
			IIntersection closestInter = null;
			foreach(IThing thing in scene.things) {
				IIntersection inter = thing.intersect(ray);
				if(inter!=null&&inter.dist<closest) {
					closestInter=inter;
					closest=inter.dist;
				}
			}
			return closestInter;
		}
		double testRay(IRay ray,IScene scene) {
			IIntersection isect = this.intersections(ray,scene);
			if(isect!=null) {
				return isect.dist;
			}
			return double.NaN;
		}
		Color traceRay(IRay ray,IScene scene,double depth) {
			IIntersection isect = this.intersections(ray,scene);
			if(isect==null) {
				return Color.background;
			}
			return this.shade(isect,scene,depth);
		}
		Color shade(IIntersection isect,IScene scene,double depth) {
			Vector d = isect.ray.dir;
			Vector pos = Vector.plus(Vector.times(isect.dist,d),isect.ray.start);
			Vector normal = isect.thing.normal(pos);
			Vector reflectDir = Vector.minus(d,Vector.times(2,Vector.times(Vector.dot(normal,d),normal)));
			Color naturalColor = Color.plus(Color.background,this.getNaturalColor(isect.thing,pos,normal,reflectDir,scene));
			Color reflectedColor = (depth>=this.maxDepth) ? Color.grey : this.getReflectionColor(isect.thing,pos,normal,reflectDir,scene,depth);
			return Color.plus(naturalColor,reflectedColor);
		}
		Color getReflectionColor(IThing thing,Vector pos,Vector normal,Vector rd,IScene scene,double depth) {
			return Color.scale(thing.surface.reflect(pos),this.traceRay(new Ray(pos,rd),scene,depth+1));
		}
		public delegate Color call_addLight(Color col,ILight light);
		Color getNaturalColor(IThing thing,Vector pos,Vector norm,Vector rd,IScene scene) {
			call_addLight addLight = (col,light) => {
				Vector ldis = Vector.minus(light.pos,pos);
				Vector livec = Vector.norm(ldis);
				double neatIsect = this.testRay(new Ray(pos,livec),scene);
				bool isInShadow = (neatIsect==double.NaN) ? false : (neatIsect<=Vector.mag(ldis));
				if(isInShadow) {
					return col;
				}
				double illum = Vector.dot(livec,norm);
				Color lcolor = (illum>0) ? Color.scale(illum,light.color) : Color.defaultColor;
				double specular = Vector.dot(livec,Vector.norm(rd));
				Color scolor = (specular>0) ? Color.scale(Math.Pow(specular,thing.surface.roughness),light.color) : Color.defaultColor;
				return Color.plus(col,Color.plus(
					Color.times(thing.surface.diffuse(pos),lcolor),
					Color.times(thing.surface.specular(pos),scolor)
				));
			};
			Color result = Color.defaultColor;
			foreach(ILight light in scene.lights) {
				result=addLight(result,light);
			}
			return result;
		}
		public delegate Vector call_getPoint(double x,double y,Camera camera);
		public delegate double call_recenter(double pos);
		public Panel render(IScene scene,double screenWidth,double screenHeight) {
			call_getPoint getPoint = (x,y,camera) => {
				call_recenter recenterX = xx => (xx-(screenWidth/2.0))/2.0/screenWidth;
				call_recenter recenterY = yy => -(yy-(screenHeight/2.0))/2.0/screenHeight;
				return Vector.norm(
					Vector.plus(camera.forward,
						Vector.plus(
							Vector.times(recenterX(x),camera.right),
							Vector.times(recenterY(y),camera.up)
						)
					)
				);
			};
			double sW = 1.0;
			double sH = 1.0;
			Canvas ug = new Canvas();
			//http://www.w3.org/TR/2dcontext/#canvasrenderingcontext2d
			for(double y = 0;y<screenHeight;y++) {
				for(double x = 0;x<screenWidth;x++) {
					Color color = this.traceRay(new Ray(scene.camera.pos,getPoint(x,y,scene.camera)),scene,0);
					Color c = Color.toDrawingColor(color);
					////////////////////////////////////////////////////////////////////////
					Rectangle re = new Rectangle();
					re.Width=1*sW;
					re.Height=1*sH;
					WpfColor rec = WpfColor.FromRgb((byte)c.r,(byte)c.g,(byte)c.b);
					re.Fill=new SolidColorBrush(rec);
					Canvas.SetLeft(re,x*sW);
					Canvas.SetTop(re,y*sH);
					ug.Children.Add(re);
					////////////////////////////////////////////////////////////////////////
				}
			}
			return ug;
		}
	}
	public class DefaultScene:IScene {
		IThing[] _things ={
			new Plane(new Vector(0.0,1.0,0.0),0.0,Surfaces.checkerboard),
			new Sphere(new Vector(0.0,1.0,-0.25),1.0,Surfaces.shiny),
			new Sphere(new Vector(-0.2,1.0,1.5),0.5,Surfaces.shiny)
		};
		ILight[] _lights ={
			new Light(new Vector(-2.0,2.5,0.0),new Color(0.49,0.07,0.07)),
			new Light(new Vector(1.5,2.5,1.5),new Color(0.07,0.07,0.49)),
			new Light(new Vector(1.5,2.5,-1.5),new Color(0.07,0.49,0.071)),
			new Light(new Vector(0.0,3.5,0.0),new Color(0.21,0.21,0.35))
		};
		Camera cam = new Camera(new Vector(3.0,2.0,4.0),new Vector(-1.0,0.5,0.0));
		public IThing[] things {
			get { return _things; }
			set { _things=value; }
		}
		public ILight[] lights {
			get { return _lights; }
			set { _lights=value; }
		}
		public Camera camera {
			get { return cam; }
			set { cam=value; }
		}
	}
}
