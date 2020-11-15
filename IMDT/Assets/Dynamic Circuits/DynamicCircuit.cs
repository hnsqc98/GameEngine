using UnityEngine;
using System.Collections.Generic;

public class DynamicCircuit : MonoBehaviour 
{
    public Material lineMaterial;
    public Material endMaterial;
    public Light headLight;
    public Vector2 lineWidth = new Vector2(0.1f, 0.1f);
    public bool drawDoubleSided = false;
    public float radiusOfLineEndObject = 1f;
    public int numberOfLines = 5;
    public float chanceOfNoTurn = 0.5f;
    public int lineLength = 5;
    public int trailingBlocks = 2;
    public Color[] colorRotation;
    public int lineNodeDensity = 1;
    public float lineSpawnDelay = 2f;
    public float timeBetweenSteps = 0.1f;
    public int viewWidth = 6;
    public int viewHeight = 4;

    private enum LineStateType
    {
        Spawning,
        Growing,
        Running,
        Blocked,
        Fading,
        Leaving,
        Deleting
    }
    private class IntPair
    {
        public int X;
        public int Y;

        public Vector2 Offset;
        public bool SpawnPoint = false;

        public IntPair() { Offset = Vector2.zero; }
        public IntPair(int x, int y) { X = x; Y = y; Offset = Vector2.zero; }
    }
    private class CircuitLineData
    {
        public GameObject Start;
        public Light LineHead;
        public LineRenderer Line;
        public GameObject End;
        public float StartAge = 0f;
        public float EndAge = 0f;
        public int Direction;
        public float StepStartTime;
        public int NumSegments = 0;
        public int Number;
        public LineStateType LineState = LineStateType.Spawning;
        public LinkedList<IntPair> Nodes;
        public IntPair QueuedNode = null;
        public int LineDecay = 1;
        public Color StartColor;
        public Color EndColor;
    }
    private LinkedList<CircuitLineData> lineList;
    private float lastLineSpawnTime = 0f;
    private int[,] nodeGraph;
    private int currentColorIndex = 0;
    private GameObject lineEnd;
    private int gridWidth;
    private int gridHeight;

	void Start () 
    {
        lineList = new LinkedList<CircuitLineData>();

        if(colorRotation.Length < 2)
            colorRotation = new Color[] { new Color(0.5f, 0.5f, 1f, 1f), new Color(0f, 0f, 1f, 0f) };
	}
	
	void Update () 
    {
        if (lineList.Count < numberOfLines && Time.time >= lastLineSpawnTime + lineSpawnDelay)
        {
            CheckNodeGraph();
            SpawnALine();
            lastLineSpawnTime = Time.time;
        }

        MoveLines();
	}

    private void MoveLines()
    {
        for(LinkedListNode<CircuitLineData> CurrentLineNode = lineList.First;CurrentLineNode != null;CurrentLineNode = CurrentLineNode.Next)
        {
            if (CurrentLineNode.Value.LineState == LineStateType.Deleting)
            {
                lineList.Remove(CurrentLineNode);
                Destroy(CurrentLineNode.Value.Line.gameObject);

                //nodeGraph[CurrentLineNode.Value.Nodes.Last.Value.X, CurrentLineNode.Value.Nodes.Last.Value.Y] = 0;

               for(int i = 0; i < nodeGraph.GetLength(0);i++)
                    for (int j = 0; j < nodeGraph.GetLength(1); j++)
                        if(nodeGraph[i, j] == CurrentLineNode.Value.Number)
                            nodeGraph[i, j] = 0;

                // We deleted something from the list, so start the enumeration over.
                CurrentLineNode = lineList.First;

                // Stop the loop if deleting this one brings the number of lines to zero.
                if (CurrentLineNode == null)
                    break;
            }
            else
            {
                CircuitLineData CurrentLine = CurrentLineNode.Value;

                if (lineNodeDensity < 1)
                    lineNodeDensity = 1;

                int NumberOfSegments = lineLength * lineNodeDensity * 2;

                // Check to make sure there are the proper number of segments
                if (CurrentLine.NumSegments != NumberOfSegments)
                {
                    CurrentLine.Line.SetVertexCount(NumberOfSegments);
                    CurrentLine.NumSegments = NumberOfSegments;
                }

                if (Time.time - CurrentLine.StepStartTime > timeBetweenSteps)
                {
                    if (CurrentLine.LineState == LineStateType.Fading)
                        AgeSegment(CurrentLine);
                    else if (CurrentLine.LineState == LineStateType.Leaving)
                        ExpireSegment(CurrentLine);
                    else
                        ProgressLine(CurrentLine);
                }

                AgeLineEnds(CurrentLine);

                LinkedListNode<IntPair> CurrentNode = CurrentLine.Nodes.First;

                if (CurrentLine.Nodes.First.Next != null)
                {
                    int i = 0;

                    if (CurrentLine.LineState == LineStateType.Leaving || CurrentLine.LineState == LineStateType.Fading)
                    {
                        int NodesDecayed = (CurrentLine.LineDecay * lineNodeDensity) +
                            Mathf.CeilToInt(((Time.time - CurrentLine.StepStartTime) / timeBetweenSteps) * lineNodeDensity);
                        while (i < NodesDecayed && i < NumberOfSegments)
                        {
                            int XAdd, YAdd;
                            ConvertDirectionToOffset(CurrentLine.Direction, out XAdd, out YAdd);
                            CurrentNode.Value.Offset = new Vector2(-((float)XAdd / (float)(Mathf.Abs(XAdd) + Mathf.Abs(YAdd))) * radiusOfLineEndObject,
                                ((float)YAdd / (float)(Mathf.Abs(XAdd) + Mathf.Abs(YAdd))) * radiusOfLineEndObject);

                            CurrentLine.Line.SetPosition(i++, LocationFromIndex(CurrentNode.Value));
                        }

                        if(CurrentLine.LineHead != null)
                        {
                            Destroy(CurrentLine.LineHead);
                            CurrentLine.LineHead = null;
                        }
                    }
                    else
                    {
                        i = SetNodes(CurrentNode, CurrentLine, NumberOfSegments, i,
                            lineNodeDensity - Mathf.FloorToInt(((Time.time - CurrentLine.StepStartTime) / timeBetweenSteps) * lineNodeDensity));

                        if(CurrentLine.LineHead != null)
                            SetHead(CurrentLine);
                    }

                    while (i < NumberOfSegments)
                    {
                        if (CurrentNode.Next != null)
                            CurrentNode = CurrentNode.Next;

                        if (CurrentNode.Next != null)
                            i = SetNodes(CurrentNode, CurrentLine, NumberOfSegments, i);
                        else
                            CurrentLine.Line.SetPosition(i++, LocationFromIndex(CurrentNode.Value));
                    }
                }

            } 
        }
    }

    private void SetHead(CircuitLineData CurrentLine)
    {
        if (CurrentLine.Nodes.First.Next != null)
            CurrentLine.LineHead.transform.localPosition = Vector3.Lerp(LocationFromIndex(CurrentLine.Nodes.First.Next.Value), 
                LocationFromIndex(CurrentLine.Nodes.First.Value),
                ((Time.time - CurrentLine.StepStartTime) / timeBetweenSteps) % 1f);
    }                             

    private int SetNodes(LinkedListNode<IntPair> CurrentNode, CircuitLineData CurrentLine, int NumberOfSegments, int i, int Skip = 0)
    {
        int j = 0;
        while (j < lineNodeDensity)
        {
            if (j >= Skip)
            {

                Vector3 IntermediateStart = Vector3.Lerp(LocationFromIndex(CurrentNode.Value),
                LocationFromIndex(CurrentNode.Next.Value), (float)(j + 1) / (float)lineNodeDensity);

                Vector3 IntermediateEnd = Vector3.Lerp(LocationFromIndex(CurrentNode.Value),
                LocationFromIndex(CurrentNode.Next.Value), (float)(j) / (float)lineNodeDensity);

                if (i + j < NumberOfSegments)
                    CurrentLine.Line.SetPosition(i + j - Skip, Vector3.Lerp(IntermediateStart, IntermediateEnd,
                        (((Time.time - CurrentLine.StepStartTime) / timeBetweenSteps) * lineNodeDensity) % 1f));
            }

            j++;
        }

        return i + j - Skip;
    }

    private void SpawnALine()
    {
        CircuitLineData NewLine = new CircuitLineData();
        
        NewLine.Nodes = new LinkedList<IntPair>();

        // Look for an unused place to start the line, give up after 10 unseccessful tries.
        int Tries = 0;
        IntPair InitialPos = new IntPair();
        do
        {
            if (++Tries > 10)
                return;
            NewLine.Direction = Random.Range(0, 7);
            InitialPos.X = (Random.Range(1, gridWidth) * 2) - 1;
            InitialPos.Y = (Random.Range(1, gridHeight) * 2) - 1;
        } while (nodeGraph[InitialPos.X, InitialPos.Y] > 0);
            
        // Find an unused line number.
        NewLine.Number = 0;
        bool Found = true;
        while (Found)
        {
            Found = false;
            NewLine.Number++;
            foreach (CircuitLineData Line in lineList)
                if (Line.Number == NewLine.Number)
                    Found = true;
        }

        InitialPos.SpawnPoint = true;
        NewLine.Nodes.AddFirst(InitialPos);
        nodeGraph[InitialPos.X, InitialPos.Y] = NewLine.Number;

        GameObject LineObject = new GameObject("Line " + NewLine.Number.ToString("00"));
        LineObject.transform.position = transform.position;
        LineObject.transform.parent = transform;
        LineObject.transform.rotation = transform.rotation;
        NewLine.Line = LineObject.AddComponent<LineRenderer>();
        NewLine.Line.useWorldSpace = false;
        NewLine.Line.material = lineMaterial;
        NewLine.Line.SetWidth(lineWidth.x, lineWidth.y);

        if (lineEnd == null)
        {
            lineEnd = new GameObject("Line Ending Template");
            lineEnd.transform.parent = transform;
            lineEnd.transform.rotation = Quaternion.identity;
            lineEnd.transform.localPosition = Vector3.zero;
            lineEnd.transform.localScale = Vector3.one;
            CreatePlane(lineEnd, drawDoubleSided);
            MeshRenderer ComponentRenderer = lineEnd.AddComponent<MeshRenderer>();
            ComponentRenderer.receiveShadows = false;
            ComponentRenderer.material = endMaterial;
            ComponentRenderer.enabled = false;
        }

        NewLine.Start = (GameObject)Instantiate(lineEnd);
        NewLine.Start.transform.parent = NewLine.Line.transform;
        NewLine.Start.transform.localPosition = LocationFromIndex(InitialPos);
        NewLine.Start.transform.localScale = Vector3.one * radiusOfLineEndObject * 2f;
        NewLine.Start.name = "Start";
        NewLine.Start.GetComponent<Renderer>().enabled = true;

        NewLine.End = (GameObject)Instantiate(lineEnd);
        NewLine.End.transform.parent = NewLine.Line.transform;
        NewLine.End.transform.localPosition = LocationFromIndex(InitialPos);
        NewLine.End.transform.localScale = Vector3.one * radiusOfLineEndObject * 2f;
        NewLine.End.name = "End";

        if(headLight != null)
        {
            NewLine.LineHead = Instantiate(headLight);
            NewLine.LineHead.transform.parent = NewLine.Line.transform;
            NewLine.LineHead.transform.localPosition = LocationFromIndex(InitialPos);
            NewLine.LineHead.transform.localScale = Vector3.one;
            NewLine.LineHead.name = "Head Light";
            NewLine.LineHead.gameObject.SetActive(true);
        }

        SetLineColor(NewLine);

        lineList.AddLast(NewLine);

        SelectNewSegment(NewLine, true);
    }

    private Vector3 LocationFromIndex(IntPair Index)
    {
        return new Vector3(((Index.X - (gridWidth / 2f)) * transform.localScale.x) + Index.Offset.x,
            (-(Index.Y - (gridHeight / 2f)) * transform.localScale.y) + Index.Offset.y, 0f);
    }

    private void CreatePlane(GameObject MyObject, bool DoubleSided = false)
    {
        Vector4[] Tangents;
        Vector3[] Verts;
        Vector3[] Normals;
        Vector2[] Uv;
        int[] Triangles;

        if (!DoubleSided)
        {
            Verts = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), 
                                    new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) };
            Normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            Tangents = new Vector4[] { new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 0f, 0f, 1f), 
                                       new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 0f, 0f, 1f) };
            Uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            Triangles = new int[] { 0, 2, 3, 0, 3, 1 };
        }
        else
        {
            Verts = new Vector3[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                                    new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f),
                                    new Vector3(-0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f),
                                    new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f)};
            Normals = new Vector3[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back, 
                                      Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward };
            Tangents = new Vector4[] { new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 0f, 0f, 1f), 
                                       new Vector4(1f, 0f, 0f, 1f), new Vector4(1f, 0f, 0f, 1f),
                                       new Vector4(-1f, 0f, 0f, 1f), new Vector4(-1f, 0f, 0f, 1f), 
                                       new Vector4(-1f, 0f, 0f, 1f), new Vector4(-1f, 0f, 0f, 1f) };
            Uv = new Vector2[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f),
                                 new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(1f, 1f) };
            Triangles = new int[] { 0, 2, 3, 0, 3, 1, 4, 6, 7, 4, 7, 5 };
        }

        MeshFilter MyMeshFilter = MyObject.AddComponent<MeshFilter>();
        Mesh MyMesh = new Mesh();

        MyMesh.vertices = Verts;
        MyMesh.triangles = Triangles;
        MyMesh.uv = Uv;
        MyMesh.normals = Normals;
        MyMesh.tangents = Tangents;
        MyMesh.name = "Simple Plane";

        MyMeshFilter.mesh = MyMesh;
    }

    private void SetLineColor(CircuitLineData CurrentLine)
    {
        if (colorRotation.Length < currentColorIndex + 2)
            currentColorIndex = 0;

        CurrentLine.Line.SetColors(colorRotation[currentColorIndex], colorRotation[currentColorIndex + 1]);
        CurrentLine.StartColor = colorRotation[currentColorIndex];
        CurrentLine.EndColor = colorRotation[currentColorIndex + 1];

        if(CurrentLine.LineHead != null)
            CurrentLine.LineHead.color = CurrentLine.StartColor;

        currentColorIndex += 2;
    }

    private void AgeSegment(CircuitLineData CurrentLine)
    {
        CurrentLine.LineDecay++;
        CurrentLine.StepStartTime = Time.time;
        if (CurrentLine.Nodes.Count + CurrentLine.LineDecay > lineLength * 2)
            CurrentLine.LineState = LineStateType.Leaving;
    }

    private void AgeLineEnds(CircuitLineData CurrentLine)
    {
        if (CurrentLine.Start.GetComponent<Renderer>().enabled)
        {
            CurrentLine.StartAge += Time.deltaTime;
            float Age = CurrentLine.StartAge / (timeBetweenSteps * lineLength * 6);
            CurrentLine.Start.GetComponent<Renderer>().material.color = Color.Lerp(CurrentLine.StartColor, CurrentLine.EndColor, Age);
            if (Age >= 1f)
                CurrentLine.Start.GetComponent<Renderer>().enabled = false;
        }

        if (CurrentLine.End.GetComponent<Renderer>().enabled)
        {
            CurrentLine.EndAge += Time.deltaTime;
            float Age = CurrentLine.EndAge / (timeBetweenSteps * lineLength * 6);
            CurrentLine.End.GetComponent<Renderer>().material.color = Color.Lerp(CurrentLine.StartColor, CurrentLine.EndColor, Age);
            if (Age >= 1f)
                CurrentLine.End.GetComponent<Renderer>().enabled = false;
        }
    }

    private void ExpireSegment(CircuitLineData CurrentLine)
    {
        if (CurrentLine.Nodes.Count > 1)
        {
            nodeGraph[CurrentLine.Nodes.Last.Value.X, CurrentLine.Nodes.Last.Value.Y] = 0;
            CurrentLine.Nodes.RemoveLast();
            CurrentLine.LineDecay++;
            CurrentLine.StepStartTime = Time.time;
        }
        else // Time to remove the line.
        {
            CurrentLine.LineState = LineStateType.Deleting;
        }
    }

    private void ProgressLine(CircuitLineData CurrentLine)
    {
        if (CurrentLine.QueuedNode != null)
        {
            CurrentLine.Nodes.AddFirst(CurrentLine.QueuedNode);
            CurrentLine.QueuedNode = null;
        }
        else
        {
            SelectNewSegment(CurrentLine, false);
        }

        CurrentLine.StepStartTime = Time.time;
    }

    private void SelectNewSegment(CircuitLineData CurrentLine, bool FirstSegment, bool FirstPass = true)
    {
        chanceOfNoTurn = Mathf.Clamp(chanceOfNoTurn, 0f, 1f);

        bool TryLeft = chanceOfNoTurn == 1f ? false : true;
        bool TryRight = chanceOfNoTurn == 1f ? false : true;
        bool TryStraight = chanceOfNoTurn == 0f ? false : true;

        while (TryLeft || TryRight || TryStraight)
        {
            float TurnDirection = Random.Range(TryLeft ? -1f : 0f, TryRight ? 1f : 0f);

            if (Mathf.Abs(TurnDirection) <= chanceOfNoTurn && TryStraight)
            {
                if (IsDirectionFree(CurrentLine.Nodes.First.Value, CurrentLine.Direction, FirstPass))
                {
                    StartNewSegment(CurrentLine, FirstSegment);
                    return;
                }
                else
                    TryStraight = false;
            }
            else if (TurnDirection < 0f)
            {
                int NewDirection = CurrentLine.Direction + 1;
                if (NewDirection > 7)
                    NewDirection = 0;

                if (IsDirectionFree(CurrentLine.Nodes.First.Value, NewDirection, FirstPass))
                {
                    CurrentLine.Direction = NewDirection;
                    StartNewSegment(CurrentLine, FirstSegment);
                    return;
                }
                else
                    TryLeft = false;
            }
            else
            {
                int NewDirection = CurrentLine.Direction - 1;
                if (NewDirection < 0)
                    NewDirection = 7;

                if (IsDirectionFree(CurrentLine.Nodes.First.Value, NewDirection, FirstPass))
                {
                    CurrentLine.Direction = NewDirection;
                    StartNewSegment(CurrentLine, FirstSegment);
                    return;
                }
                else
                    TryRight = false;
            }
        }

        // If we can't find a valid direction, then run the search again without looking ahead.
        if (FirstPass)
            SelectNewSegment(CurrentLine, FirstSegment, false);
        else
        {
            // If we get to here then there are no valid directions.
            // If this is the first segment then hide the line, since it's nodes will never get set.
            if (FirstSegment)
                CurrentLine.Line.enabled = false;

            EndLineSegment(CurrentLine);
        }
    }

    private void EndLineSegment(CircuitLineData CurrentLine)
    {
        CurrentLine.End.GetComponent<Renderer>().enabled = true;
        CurrentLine.End.transform.localPosition = LocationFromIndex(CurrentLine.Nodes.First.Value);

        if (CurrentLine.Nodes.Count < (lineLength * 2))
            CurrentLine.LineState = LineStateType.Fading;
        else
            CurrentLine.LineState = LineStateType.Leaving;
    }

    private void StartNewSegment(CircuitLineData CurrentLine, bool DoOffset)
    {
        int XAdd, YAdd;
        ConvertDirectionToOffset(CurrentLine.Direction, out XAdd, out YAdd);

        // Occupy the two spots on the node graph.
        nodeGraph[CurrentLine.Nodes.First.Value.X + XAdd, CurrentLine.Nodes.First.Value.Y + YAdd] = CurrentLine.Number;
        nodeGraph[CurrentLine.Nodes.First.Value.X + XAdd + XAdd, CurrentLine.Nodes.First.Value.Y + YAdd + YAdd] = CurrentLine.Number;

        // Move to spot 1
        
        IntPair Spot1 = new IntPair(CurrentLine.Nodes.First.Value.X + XAdd, CurrentLine.Nodes.First.Value.Y + YAdd);
        
        // Add an offset if requested
        if (DoOffset)
            Spot1.Offset = new Vector2(((float)XAdd / (float)(Mathf.Abs(XAdd) + Mathf.Abs(YAdd))) * radiusOfLineEndObject,
                -((float)YAdd / (float)(Mathf.Abs(XAdd) + Mathf.Abs(YAdd))) * radiusOfLineEndObject);

        CurrentLine.Nodes.AddFirst(Spot1);

        // Queue spot 2
        // Note: Nodes.First has been updated from the line above.
        IntPair QueuedNode = new IntPair(CurrentLine.Nodes.First.Value.X + XAdd, CurrentLine.Nodes.First.Value.Y + YAdd);
        CurrentLine.QueuedNode = QueuedNode;

        CurrentLine.StepStartTime = Time.time;

        // Check for user stupidity real quick
        if (lineLength < 0)
            lineLength = 0;
        if (trailingBlocks < 0)
            trailingBlocks = 0;

        // Erace the oldest blocks if the line has now gotten too long
        for (int i = CurrentLine.Nodes.Count - (((lineLength + trailingBlocks) * 2) - 1); i > 0; i--)
            CurrentLine.Nodes.RemoveLast();
    }

    private void PrintNodeGraph()
    {
        string PrintStr = string.Empty;

        for (int i = 0; i < nodeGraph.GetLength(1); i++)
        {
            for (int j = 0; j < nodeGraph.GetLength(0); j++)
            {
                if(nodeGraph[j, i] >= 0)
                    PrintStr += nodeGraph[j, i].ToString(" 00") + " ";
                else
                    PrintStr += nodeGraph[j, i].ToString("00") + " ";
            }
            PrintStr += "\n";
        }

        print(PrintStr);
    }

    // Direction:
    // 701
    // 6-2
    // 543
    private void ConvertDirectionToOffset(int Direction, out int XOffset, out int YOffset)
    {
        if (Direction < 0)
            Direction += 8;

        switch (Direction % 8)
        {
            case 0:
                XOffset = 0;
                YOffset = -1;
                break;
            case 1:
                XOffset = 1;
                YOffset = -1;
                break;
            case 2:
                XOffset = 1;
                YOffset = 0;
                break;
            case 3:
                XOffset = 1;
                YOffset = 1;
                break;
            case 4:
                XOffset = 0;
                YOffset = 1;
                break;
            case 5:
                XOffset = -1;
                YOffset = 1;
                break;
            case 6:
                XOffset = -1;
                YOffset = 0;
                break;
            case 7:
                XOffset = -1;
                YOffset = -1;
                break;
            default:
                Debug.LogError("Direction is out of range! (" + Direction.ToString() + ")");
                XOffset = 0;
                YOffset = 0;
                break;
        }
    }

    private bool IsDirectionFree(IntPair Position, int Direction, bool LookTwoPlaces)
    {
        int XAdd, YAdd;

        ConvertDirectionToOffset(Direction, out XAdd, out YAdd);

        if (nodeGraph[Position.X + XAdd, Position.Y + YAdd] == 0 &&
            nodeGraph[Position.X + (XAdd * 2), Position.Y + (YAdd * 2)] == 0)
        {
            if (LookTwoPlaces)
            {
                IntPair ForwardPosition = new IntPair(Position.X + (XAdd * 2), Position.Y + (YAdd * 2));
                if (IsDirectionFree(ForwardPosition, Direction, false) ||
                    IsDirectionFree(ForwardPosition, Direction + 1, false) ||
                    IsDirectionFree(ForwardPosition, Direction - 1, false))
                    return true;
            }
            else
                return true;
        }

        return false;
    }

    private void CheckNodeGraph()
    {
        // Sanity check
        if (viewWidth < 1)
            viewWidth = 1;
        if (viewHeight < 1)
            viewHeight = 1;

        if (nodeGraph == null)
        {
            nodeGraph = new int[(viewWidth * 2) + 1, (viewHeight * 2) + 1];
            for (int i = 0; i < nodeGraph.GetLength(0); i++)
                for (int j = 0; j < nodeGraph.GetLength(1); j++)
                {
                    // Do the outer edges
                    if ((i == 0 || i >= viewWidth * 2) || (j == 0 || j >= viewHeight * 2))
                        nodeGraph[i, j] = -1;
                    else // Inner parts
                        nodeGraph[i, j] = 0;
                }
            gridWidth = viewWidth;
            gridHeight = viewHeight;
        }
        else if (gridWidth != viewWidth || gridHeight != viewHeight)
        {
            int currentNodeGraphWidth = nodeGraph.GetLength(0);
            int currentNodeGraphHeight = nodeGraph.GetLength(1);

            // Check to see if the array needs to be resized.
            // if it's not resized than newNodeGraph will just be the same array as nodeGraph
            int[,] newNodeGraph = nodeGraph;
            if ((currentNodeGraphWidth * 2) + 1 < viewWidth || (currentNodeGraphHeight * 2) + 1 < viewHeight)
                newNodeGraph = new int[(viewWidth * 2) + 1, (viewHeight * 2) + 1];

            // Now copy over data while redrawing the -1 squares
            for (int i = 0; i < newNodeGraph.GetLength(0); i++)
                for (int j = 0; j < newNodeGraph.GetLength(1); j++)
                {
                    if (i < nodeGraph.GetLength(0) && j < nodeGraph.GetLength(1))
                    {
                        // Do the outer edges
                        if ((i == 0 || i >= gridWidth * 2) || (j == 0 || j >= gridHeight * 2))
                            newNodeGraph[i, j] = nodeGraph[i, j] <= 0 ? -1 : nodeGraph[i, j];
                        else // Inner parts
                            newNodeGraph[i, j] = nodeGraph[i, j] == -1 ? 0 : nodeGraph[i, j];
                    }
                    else
                    {
                        // Do the outer edges
                        if ((i == 0 || i >= gridWidth * 2) || (j == 0 || j >= gridHeight * 2))
                            newNodeGraph[i, j] = -1;
                        else // Inner parts
                            newNodeGraph[i, j] = 0;
                    }
                }

            gridWidth = viewWidth;
            gridHeight = viewHeight;

            nodeGraph = newNodeGraph;
        }
    }
}
