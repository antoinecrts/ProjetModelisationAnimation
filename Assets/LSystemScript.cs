using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;

/* classe permettant de stocker la position et la rotation pour un objet */
public class TransformInfo
{
	public Vector3 position;
	public Quaternion rotation;
}

public class LSystemScript : MonoBehaviour {

	/* compléxité de la structure de la plante */
	[SerializeField] private int iterations = 4;

	/* le composant line renderer permet de représenter des branches */
	[SerializeField] private GameObject Branch;

	/* longueur des branches */
	[SerializeField] private float length = 1f;

	/* angle pour changer l'orientation des branches */
	[SerializeField] private float angle = 30f;

	/* Toutes les structures de plante utilisées ont pour axiome "X" */
	private const string axiom = "X";

	/* pile pour sauvegarder et revenir à la position et orientation d'une branche précédente */
	private Stack<TransformInfo> transform_stack;

	/* le l-système est caractérisée par un axiom (vu précdemment) et des règles */
	private Dictionary<char, string> rules;

	/* représentation de la structure de la plante */
	private string current_string = string.Empty;

	/* objet sur lequel la plante va s'adapter */
	private GameObject sphere;

	/* permet de gérer les collisions */
	private SphereCollider coll;

	/* vitesse de croissance des plantes */
	public float vi;

	/* permet de choisir quelle structure de plante utiliser */
	public int shape;

	/* capacité d'adaptation de la plante à la surface (ici la sphère) */
	public float adaptation_strength;

	// Initialisation
	void Start () {

		transform_stack = new Stack<TransformInfo>();

		/* Changements des regles pour avoir differentes structures pour la plante */
		
		// Première structure de plante grimpante
		/* l'axiome est X
		   Règles : 'X', "[F-[[X]+X]+F[+FX]-X]" 
		   			'F', "FF"
		*/

		// Seconde structure de plante
		/* l'axiome est toujours X 
		   Règles :'X' , "F+[-F-XF-X][+FF][--XF[+X]][++F-X]"
		   		   'F' , "FF"
		*/

		// Troisième structure de plante 
		/* l'axiome est toujours X 
		   Règles :'X' , "FF[+XZ++X-F[+ZX]][-X++F-X]"
		   		   'F' , "FX[FX[+XF]]"
				   'Z' , "[+F-X-F][++ZX]"
		*/

		if (shape == 1) {
			rules = new Dictionary<char, string> {
				{'X', "[F-[[X]+X]+F[+FX]-X]"},
				{'F', "FF"}
			};
		} else if (shape == 2) {
			rules = new Dictionary<char, string> {
				{'X', "F+[-F-XF-X][+FF][--XF[+X]][++F-X]"},
				{'F', "FF"}
			};
		} else {
			rules = new Dictionary<char, string> {
				{'X', "FF[+XZ++X-F[+ZX]][-X++F-X]"},
				{'Z', "[+F-X-F][++ZX]"},
				{'F', "FX[FX[+XF]]"}
			};
		}

		/* Un objet présent dans la scène ici une sphère */
		sphere = GameObject.Find("Sphere");

		/* Permet de gérer les collisions avec l'objet */
		coll = sphere.GetComponent<SphereCollider>();

		/* Permet de gérer le vitesse de croissance de la plante */
		StartCoroutine(Generate());

	}


	private IEnumerator Generate() {

		/* à la première étape il y a juste l'axiome soit "X" */
		current_string = axiom ;

		StringBuilder s_b = new StringBuilder();

		for (int i = 0; i < iterations; i++) {
			foreach(char c in current_string) {
				if (rules.ContainsKey(c)){
					s_b.Append(rules[c]);
				} else {
					s_b.Append(c.ToString());
				}
			}
			current_string = s_b.ToString();

			s_b = new StringBuilder();
		}

		/* le current_string représente la structure de l'arbre et en fonction du caractère lu différentes actions sont effectuées */

		foreach(char c in current_string) {

			switch(c) {
				
				/* une branche est crée */
				case 'F':

					/* temps à attendre avant qu'une nouvelle branche soit crée */
					yield return new WaitForSeconds(vi);

					/* position du premier point de la branche */
					Vector3 initial_position = transform.position;

					/* position du second point de la branche */
					transform.Translate(Vector3.up * length);

					/* Adaptation à la surface */

					/* direction du dernier point de la branche vers le point le plus proche de la sphère */
					Vector3 dir = coll.ClosestPoint(transform.position) - transform.position;
					Vector3 axis = Vector3.Cross(dir.normalized, transform.up);
					/* la capacité de la plante à s'adapter à la surface (ici une sphère) est paramétré par adaptation_strength */
					float ang = Vector3.Dot(dir.normalized, transform.up) * adaptation_strength;

					transform.Rotate(Quaternion.AngleAxis(ang, axis).eulerAngles);

					/* Une branche est représenté sous la forme d'une ligne entre deux points */
					GameObject treeSegment = Instantiate(Branch);
					treeSegment.GetComponent<LineRenderer>().SetPosition(0, initial_position);
					treeSegment.GetComponent<LineRenderer>().SetPosition(1, transform.position);
					
					break;	

				/* rien n'est fait lorsque ces caractères sont vues */
				case 'X':
					break;

				case 'Z':
					break;

				/* + et - permettent d'orienter la plante dans une direction respectivement vers la droite ou vers la gauche */
				case '+':
					transform.Rotate((-transform.forward) * angle);
					break;

				case '-':
					transform.Rotate(transform.forward * angle);
					break;

				/* Permet d'enregistrer la position et l'orientation de l'objet */
				case '[':
					transform_stack.Push(new TransformInfo() 
					{
						position = transform.position,
						rotation = transform.rotation
					});
					break;

				/* Permet de revenir à l'orientation et la position précédente de l'objet */
				case ']':
					TransformInfo t_i = transform_stack.Pop();
					transform.position = t_i.position;
					transform.rotation = t_i.rotation;
					break;

				default:
					throw new InvalidOperationException("Problème lors de la construction de la plante");


			}

		}
		

	}
	
}

// Tutoriel utilisé pour l'implémentation de cette approche : // 
/* https://www.youtube.com/watch?v=tUbTGWl-qus&ab_channel=PeteP */