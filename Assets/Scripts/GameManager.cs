using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Size;
    public GameObject Panel;
    public GameObject token;
    private Node [,] NodeMatrix;
    private int startPosx, startPosy;
    private int endPosx, endPosy;

    void Awake ()
    {
        Instance = this;
        Calculs.CalculateDistances (Panel.GetComponent<BoxCollider2D>(), Size);

    }

    private void Start ()
    {
        startPosx = Random.Range (0, Size);
        startPosy = Random.Range (0, Size);
        do
        {
            endPosx = Random.Range (0, Size);
            endPosy = Random.Range (0, Size);
        } while (endPosx == startPosx || endPosy == startPosy);

        NodeMatrix = new Node [Size, Size];
        CreateNodes ();
    }

    public void CreateNodes ()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                NodeMatrix [i, j] = new Node (i, j, Calculs.CalculatePoint (i, j));
                NodeMatrix [i, j].Heuristic = Calculs.CalculateHeuristic (NodeMatrix [i, j], endPosx, endPosy);
            }
        }
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                SetWays (NodeMatrix [i, j], i, j);
            }
        }
        Instantiate(token, NodeMatrix [startPosx, startPosy].RealPosition, Quaternion.identity);
        Instantiate (token, NodeMatrix [endPosx, endPosy].RealPosition, Quaternion.identity);
        //DebugMatrix ();
        Pathfinding ();
    }

    public void DebugMatrix ()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                Instantiate (token, NodeMatrix [i, j].RealPosition, Quaternion.identity);
                Debug.Log ("Element (" + j + ", " + i + ")");
                Debug.Log ("Position " + NodeMatrix [i, j].RealPosition);
                Debug.Log ("Heuristic " + NodeMatrix [i, j].Heuristic);
                Debug.Log ("Ways: ");
                foreach (var way in NodeMatrix [i, j].WayList)
                {
                    Debug.Log (" (" + way.NodeDestiny.PositionX + ", " + way.NodeDestiny.PositionY + ")");
                }
            }
        }
    }

    public void SetWays (Node node, int x, int y)
    {
        node.WayList = new List<Way> ();
        if (x > 0)
        {
            node.WayList.Add (new Way (NodeMatrix [x - 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add (new Way (NodeMatrix [x - 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (x < Size - 1)
        {
            node.WayList.Add (new Way (NodeMatrix [x + 1, y], Calculs.LinearDistance));
            if (y > 0)
            {
                node.WayList.Add (new Way (NodeMatrix [x + 1, y - 1], Calculs.DiagonalDistance));
            }
        }
        if (y > 0)
        {
            node.WayList.Add (new Way (NodeMatrix [x, y - 1], Calculs.LinearDistance));
        }
        if (y < Size - 1)
        {
            node.WayList.Add (new Way (NodeMatrix [x, y + 1], Calculs.LinearDistance));
            if (x > 0)
            {
                node.WayList.Add (new Way (NodeMatrix [x - 1, y + 1], Calculs.DiagonalDistance));
            }
            if (x < Size - 1)
            {
                node.WayList.Add (new Way (NodeMatrix [x + 1, y + 1], Calculs.DiagonalDistance));
            }
        }
    }

    public void Pathfinding ()
    {
        List<Node> aVisitar = new List<Node> ();
        List<Node> Visitats = new List<Node> ();

        Node currentNode = NodeMatrix[startPosx, startPosy];
        aVisitar.Add (currentNode);
        while (aVisitar.Count > 0)
        {
            aVisitar = aVisitar.OrderBy(n => GetFCost(n)).ToList();
            currentNode = aVisitar[0];
            aVisitar.RemoveAt(0);
            Visitats.Add(currentNode);
            if (currentNode == NodeMatrix [endPosx, endPosy])
            {
                print ("camino Encontrado");
                StartCoroutine (PaintWay (currentNode));
                break;
            }
            foreach (var way in currentNode.WayList)
            {
                Node vecino = way.NodeDestiny;
                if (Visitats.Contains (vecino))
                {
                    continue;
                }
                float cost = way.ACUMulatedCost + way.Cost;
                if (!aVisitar.Contains (vecino))
                {
                    vecino.NodeParent = currentNode;
                    way.ACUMulatedCost = cost;
                    aVisitar.Add (vecino);
                }
            }
        }
        Debug.Log ("Path not found");

    }
    private float GetFCost (Node node)
    {
        foreach (var way in node.WayList)
        {
            if (way.NodeDestiny == node)
                return node.Heuristic + way.ACUMulatedCost + way.Cost;
        }
        return Mathf.Infinity;
    }

    private IEnumerator PaintWay (Node node)
    {
        Stack<Node> pathStack = new Stack<Node> ();

        while (node != null)
        {
            pathStack.Push (node);
            node = node.NodeParent;
        }

        while (pathStack.Count > 0)
        {
            Node currentNode = pathStack.Pop ();
            Instantiate (token, currentNode.RealPosition, Quaternion.identity);
            yield return new WaitForSeconds (0.1f);
        }
    }
}
