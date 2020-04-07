using UnityEngine;

public class MeshLineGenerator : MonoBehaviour
{
	public Vector3[] LinePoints;
	public Vector3[] LineNormals;
	private int numPoints;
	private int numSegments;

	public Vector3[] NewVerts;
	public Vector2[] NewUV;
	public int[] NewTriangles;

	private MeshFilter meshFilter;
	private Mesh mesh;
	private Transform tr;

	private float lineWidth = 0.5f;
	private float groundYOffset = 0.1f;

	private void Awake()
	{
		meshFilter = GetComponent<MeshFilter>();
		mesh = meshFilter.mesh;
		tr = GetComponent<Transform>();
	}

	// ************************************************************************
	// 		PUBLIC ACCESSORS
	// ************************************************************************
	public void SetLineWidth(float width)
	{
		lineWidth = width;
	}
	public void SetGroundOffset(float offset)
	{
		groundYOffset = offset;
	}

	public void SetLinePoints(Vector3[] points, Vector3[] normals, float offset)
	{
		SetGroundOffset(offset);
		SetLinePoints(points, normals);
	}
	public void SetLinePoints(Vector3[] points, Vector3[] normals)
	{
		LinePoints = points;
		LineNormals = normals;
		Vector3 yOff = new Vector3(0f, groundYOffset, 0f);
		for (int i = 0; i < LinePoints.Length; i++)
		{
			LinePoints[i] += yOff;
		}
		numPoints = LinePoints.Length;
		numSegments = numPoints - 1;
	}

	public void GenerateMesh()
	{
		rebuildMesh();
	}

	// ************************************************************************
	// 		MESH BUILDING OPERATIONS
	// ************************************************************************
	private void rebuildMesh()
	{
		NewVerts = new Vector3[4 + (numSegments - 1) * 2];
		NewUV = new Vector2[4 + (numSegments - 1) * 2];

		Vector3 xformL, localPointL = new Vector3(-lineWidth / 2, 0f, 0f);
		Vector3 xformR, localPointR = new Vector3(lineWidth / 2, 0f, 0f);

		Vector3 worldPoint1, worldPoint2, worldPoint3;

		Vector3 v1, v2, v3, v4;

		// initiate the first two triangles manually, so set proper initial path direction
		// and to more easily loop remaining new points in each respective triangle
		worldPoint1 = LinePoints[0];
		worldPoint2 = LinePoints[1];

		Vector3[] newPoints = steerNewLineSegments(worldPoint1, worldPoint1 + (worldPoint2 - worldPoint1), localPointL, localPointR);
		Vector3[] newOffset = alignNewLineSegmentsWithNormal(worldPoint1, worldPoint1 + (worldPoint2 - worldPoint1), LineNormals[0]);

		xformL = newPoints[0] + newOffset[0];
		xformR = newPoints[1] + newOffset[1]; ;

		v1 = worldPoint1 + xformL;
		v2 = worldPoint1 + xformR;

		NewVerts[0] = v1 - tr.position;
		NewVerts[1] = v2 - tr.position;

		if (LinePoints.Length > 2)
		{
			newPoints = steerNewLineSegments2ndOrder(worldPoint1, worldPoint2, LinePoints[2], localPointL, localPointR);
			newOffset = alignNewLineSegmentsWithNormal(worldPoint1, worldPoint2, LineNormals[1]);
		}
		else
		{
			newPoints = steerNewLineSegments(worldPoint1, worldPoint2, localPointL, localPointR);
			newOffset = alignNewLineSegmentsWithNormal(worldPoint1, worldPoint2, LineNormals[1]);
		}

		xformL = newPoints[0] + newOffset[0];
		xformR = newPoints[1] + newOffset[1];

		v3 = worldPoint2 + xformL;
		v4 = worldPoint2 + xformR;

		NewVerts[2] = v3 - tr.position;
		NewVerts[3] = v4 - tr.position;

		NewUV[0] = new Vector2(NewVerts[0].x, NewVerts[0].z);
		NewUV[1] = new Vector2(NewVerts[2].x, NewVerts[2].z);

		for (int i = 1; i < numSegments; i++)
		{
			v1 = v3;
			v2 = v4;

			worldPoint1 = worldPoint2;
			worldPoint2 = LinePoints[i + 1];

			if (i < (numSegments - 1))
			{
				worldPoint3 = LinePoints[i + 2];
			}
			else
			{
				worldPoint3 = worldPoint2 + (worldPoint2 - worldPoint1);
			}
			// perform y-axis rotation to align local points with direction line is moving
			// perform x and z-axis rotations to align local points with surface normal
			newPoints = steerNewLineSegments2ndOrder(worldPoint1, worldPoint2, worldPoint3, localPointL, localPointR);
			newOffset = alignNewLineSegmentsWithNormal(worldPoint1, worldPoint2, LineNormals[i + 1]);

			xformL = newPoints[0] + newOffset[0];
			xformR = newPoints[1] + newOffset[1];

			v3 = worldPoint2 + xformL;
			v4 = worldPoint2 + xformR;

			NewVerts[2 * i + 2] = v3 - tr.position;
			NewVerts[2 * i + 3] = v4 - tr.position;

			NewUV[2 * i] = new Vector2(NewVerts[i].x, NewVerts[i].z);
			NewUV[2 * i + 1] = new Vector2(NewVerts[i + 2].x, NewVerts[i + 2].z);
		}

		NewTriangles = new int[numSegments * 6];
		for (int i = 0; i < (numSegments); i++)
		{
			NewTriangles[i * 6] = i * 2 + 1;
			NewTriangles[i * 6 + 1] = i * 2;
			NewTriangles[i * 6 + 2] = i * 2 + 2;

			NewTriangles[i * 6 + 3] = i * 2 + 1;
			NewTriangles[i * 6 + 4] = i * 2 + 2;
			NewTriangles[i * 6 + 5] = i * 2 + 3;
		}

		mesh.Clear();
		mesh.vertices = NewVerts;
		mesh.uv = NewUV;
		mesh.triangles = NewTriangles;
	}

	// ***********************************************************************************************
	// 			Mathematical HELPeR Functions
	// ***********************************************************************************************
	private Vector3[] steerNewLineSegments2ndOrder(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 offsetL, Vector3 offsetR)
	{
		Vector3 dir1 = p2 - p1;
		Vector3 dir2 = p3 - p2;

		Vector3[] offsets = new Vector3[2];

		offsets[0] = offsetL;
		offsets[1] = offsetR;

		float roteAngle = Vector3.Angle(new Vector3(0f, 0f, 1f), dir1);
		if (dir1.x > 0)
		{
			roteAngle = 360f - roteAngle;
		}

		float insetAngle = Vector3.Angle(dir1, dir2);
		if (targetIsOnLEFT(dir1, dir2)) { insetAngle = -insetAngle; }

		roteAngle += insetAngle / 2f;

		Vector3 temp;
		for (int i = 0; i < offsets.Length; i++)
		{
			temp = offsets[i];
			temp.x = Mathf.Cos(roteAngle * Mathf.Deg2Rad) * offsets[i].x - Mathf.Sin(roteAngle * Mathf.Deg2Rad) * offsets[i].z;
			temp.z = Mathf.Sin(roteAngle * Mathf.Deg2Rad) * offsets[i].x + Mathf.Cos(roteAngle * Mathf.Deg2Rad) * offsets[i].z;
			offsets[i] = temp;
		}

		return offsets;
	}


	private Vector3[] steerNewLineSegments(Vector3 p1, Vector3 p2, Vector3 offsetL, Vector3 offsetR)
	{
		Vector3 dir = p2 - p1;
		Vector3[] offsets = new Vector3[2];

		offsets[0] = offsetL;
		offsets[1] = offsetR;

		float roteAngle = Vector3.Angle(new Vector3(0f, 0f, 1f), dir);
		if (dir.x < 0)
		{
			roteAngle = 360f - roteAngle;
		}
		Vector3 temp;
		for (int i = 0; i < offsets.Length; i++)
		{
			temp = offsets[i];
			temp.x = Mathf.Cos(roteAngle * Mathf.Deg2Rad) * offsets[i].x - Mathf.Sin(roteAngle * Mathf.Deg2Rad) * offsets[i].z;
			temp.z = Mathf.Sin(roteAngle * Mathf.Deg2Rad) * offsets[i].x + Mathf.Cos(roteAngle * Mathf.Deg2Rad) * offsets[i].z;
			offsets[i] = temp;
		}

		return offsets;
	}

	private Vector3[] alignNewLineSegmentsWithNormal(Vector3 p1, Vector3 p2, Vector3 n)
	{
		Vector3 dir = p2 - p1;
		Vector3[] offsets = new Vector3[2];

		Vector3 rightSide = (Vector3.Cross(n, dir).normalized * lineWidth / 2);
		Vector3 leftSide = -rightSide;

		float rightSideY = rightSide.y;
		float leftSideY = -rightSideY;

		offsets[0] = new Vector3(0f, leftSideY, 0f);
		offsets[1] = new Vector3(0f, rightSideY, 0f);

		return offsets;
	}


	// ****************************************************************************
	// FUNCTION targetIsOnLEFT - determines which side the vector v2 is on
	//				returns a bool to make this packageable in IF stmnts
	// 		inputs: v1, v2
	// 		output: TRUE if v1 is on the LEFT of unit v2
	private bool targetIsOnLEFT(Vector3 v1, Vector3 v2)
	{
		// find out if v1 is on left or right side of v2
		float d, x, x2, y, y2;
		x = v1.x;
		y = v1.z;
		x2 = v2.x;
		y2 = v2.z;

		d = (x) * (y2) - (y) * (x2);

		return (d < 0);
	}
}
