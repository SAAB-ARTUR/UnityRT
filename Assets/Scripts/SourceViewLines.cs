using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SourceVewLines : MonoBehaviour
{
    [SerializeField] Material lineMaterial = null;
    [SerializeField] float lineLength = 1;

    LineRenderer srcDirectionLine;
    LineRenderer srcViewLine1;
    LineRenderer srcViewLine2;
    LineRenderer srcViewLine3;
    LineRenderer srcViewLine4;

    const float PI = 3.14159265f;

    private LineRenderer CreateSrcViewLine(string name)
    {
        LineRenderer viewLine = new GameObject(name).AddComponent<LineRenderer>();
        viewLine.startWidth = 0.01f;
        viewLine.endWidth = 0.01f;
        viewLine.positionCount = 2;
        viewLine.useWorldSpace = true;

        viewLine.material = lineMaterial;
        viewLine.material.color = Color.black;

        return viewLine;
    }

    private void UpdateSourceViewLines()
    {
        Vector3[] viewLines = ViewLines();

        srcViewLine1.SetPosition(0, this.transform.position);
        srcViewLine1.SetPosition(1, this.transform.position + viewLines[0] * lineLength);

        srcViewLine2.SetPosition(0, this.transform.position);
        srcViewLine2.SetPosition(1, this.transform.position + viewLines[1] * lineLength);

        srcViewLine3.SetPosition(0, this.transform.position);
        srcViewLine3.SetPosition(1, this.transform.position + viewLines[2] * lineLength);

        srcViewLine4.SetPosition(0, this.transform.position);
        srcViewLine4.SetPosition(1, this.transform.position + viewLines[3] * lineLength);
    }

    Vector3[] ViewLines()
    {
        // angles for srcSphere's forward vector (which is of length 1 meaning that r can be removed from all equations below)

        SourceParams srcParams = this.GetComponent<SourceParams>();

        float origin_theta = (float)Math.Acos(this.transform.forward.y);
        float origin_phi = (float)Math.Atan2(this.transform.forward.z, this.transform.forward.x);

        float theta_rad = srcParams.theta * PI / 180; //convert to radians
        float phi_rad = srcParams.theta * PI / 180;

        float s0 = (float)Math.Sin(origin_phi);
        float c0 = (float)Math.Cos(origin_phi);

        // create angular spans in both dimensions
        float[] theta_offsets = new float[2] { origin_theta - theta_rad / 2, origin_theta + theta_rad / 2 };
        float[] phi_offsets = new float[2] { origin_phi - phi_rad / 2, origin_phi + phi_rad / 2 };

        Vector3[] viewLines = new Vector3[4];

        int k = 0;
        for (int i = 0; i < 2; i++) // loop over phi
        {
            float s1 = (float)Math.Sin(phi_offsets[i] - origin_phi);
            float c1 = (float)Math.Cos(phi_offsets[i] - origin_phi);

            for (int j = 0; j < 2; j++) // loop over theta
            {
                float x = c0 * c1 * (float)Math.Sin(theta_offsets[j]) - s0 * s1;
                float z = s0 * c1 * (float)Math.Sin(theta_offsets[j]) + c0 * s1;
                float y = c1 * (float)Math.Cos(theta_offsets[j]);
                viewLines[k] = new Vector3(x, y, z);
                k++;
            }
        }

        return viewLines;
    }

    // Start is called before the first frame update
    void Start()
    {
        srcDirectionLine = CreateSrcViewLine("SourceDirectionLine");

        srcDirectionLine.SetPosition(0, this.transform.position);
        srcDirectionLine.SetPosition(1, this.transform.position + this.transform.forward * lineLength);

        srcDirectionLine.material = lineMaterial;
        srcDirectionLine.material.color = Color.black;

        Vector3[] viewLines = ViewLines();

        // line1
        srcViewLine1 = CreateSrcViewLine("View line1");

        srcViewLine1.SetPosition(0, this.transform.position);
        srcViewLine1.SetPosition(1, this.transform.position + viewLines[0] * lineLength);

        // line2
        srcViewLine2 = CreateSrcViewLine("View line2");

        srcViewLine2.SetPosition(0, this.transform.position);
        srcViewLine2.SetPosition(1, this.transform.position + viewLines[1] * lineLength);

        // line3
        srcViewLine3 = CreateSrcViewLine("View line3");

        srcViewLine3.SetPosition(0, this.transform.position);
        srcViewLine3.SetPosition(1, this.transform.position + viewLines[2] * lineLength);

        // line4
        srcViewLine4 = CreateSrcViewLine("View line4");

        srcViewLine4.SetPosition(0, this.transform.position);
        srcViewLine4.SetPosition(1, this.transform.position + viewLines[3] * lineLength);
    }

    // Update is called once per frame
    void Update()
    {
        if (srcDirectionLine != null)
        {
            srcDirectionLine.SetPosition(0, this.transform.position);
            srcDirectionLine.SetPosition(1, this.transform.position + this.transform.forward * lineLength);

            UpdateSourceViewLines();
        }
    }
}
