using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectLoader : MonoBehaviour
{

    private string directoryPath;
    private bool isLoaded = true;

    void Awake()
    {
        isLoaded = true;
    }

    public void Load(string path, string filename)
    {
        if (!isLoaded)
            return;

        directoryPath = path;
        StartCoroutine(ConstructModel(filename));
    }

    IEnumerator ConstructModel(string filename)
    {
        isLoaded = false;

        FileReader.ObjectFile obj = FileReader.ReadObjectFile(directoryPath + filename);
        
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        
        filter.mesh = PopulateMesh(obj);
        filter.name = Path.GetFileNameWithoutExtension(filename);

        if (File.Exists(Path.Combine(directoryPath + obj.mtllib)))
        {
            FileReader.MaterialFile mtl = FileReader.ReadMaterialFile(directoryPath + obj.mtllib);
            renderer.materials = DefineMaterial(obj, mtl);
        } 
        else
        {
            renderer.materials = DefineMaterial();
        }

        isLoaded = true;
        yield return null;
    }

    Mesh PopulateMesh(FileReader.ObjectFile obj)
    {
        Mesh mesh = new Mesh();

        List<int[]> triplets = new List<int[]>();
        List<int> submeshes = new List<int>();

        for (int i = 0; i < obj.f.Count; ++i)
        {
            for (int j = 0; j < obj.f[i].Count; ++j)
            {
                triplets.Add(obj.f[i][j]);
            }
            submeshes.Add(obj.f[i].Count);
        }

        Vector3[] vertices = new Vector3[triplets.Count];
        Vector3[] normals = new Vector3[triplets.Count];
        Vector2[] uvs = new Vector2[triplets.Count];
        Color[] colors = new Color[triplets.Count];

        for (int i = 0; i < triplets.Count; ++i)
        {
            vertices[i] = obj.v[triplets[i][0] - 1];
            if (obj.vn.Count > 0)
            {
                normals[i] = obj.vn[triplets[i][2] - 1];
            }
            
            if (obj.vt.Count > 0)
            {
                if (triplets[i][1] > 0)
                    uvs[i] = obj.vt[triplets[i][1] - 1];
            }

            if (obj.vc.Count > 0)
            {
                Vector3 color = obj.vc[triplets[i][0] - 1];
                colors[i] = new Color(color.x, color.y, color.z);
            }
        }

        mesh.name = "default";
        mesh.indexFormat = (vertices.Length > ushort.MaxValue) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.subMeshCount = submeshes.Count;

        int vertex = 0;
        for (int i = 0; i < submeshes.Count; ++i)
        {
            int[] triangles = new int[submeshes[i]];
            for (int j = 0; j < submeshes[i]; ++j)
            {
                triangles[j] = vertex;
                ++vertex;
            }
            mesh.SetTriangles(triangles, i);
        }

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    Material[] DefineMaterial(FileReader.ObjectFile obj, FileReader.MaterialFile mtl)
    {
        Material[] materials = new Material[obj.usemtl.Count];

        for (int i = 0; i < obj.usemtl.Count; ++i)
        {
            int index = mtl.newmtl.IndexOf(obj.usemtl[i]);

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(File.ReadAllBytes(directoryPath + mtl.mapKd[index]));

            //materials[i] = new Material(Shader.Find("Diffuse"))
            materials[i] = new Material(Shader.Find("Standard (Specular setup)"))
            {
                name = mtl.newmtl[index],
                mainTexture = texture
            };
        }

        return materials;
    }

    Material[] DefineMaterial()
    {
        Material[] materials = new Material[1];
        materials[0] = new Material(Shader.Find("Particles/Standard Unlit"));
        return materials;
    }
}
