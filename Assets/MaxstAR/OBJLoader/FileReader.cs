/*
 * The .obj and .mtl files must follow Wavefront OBJ Specifications.
 *
 * OBJ Supported List
 *
 *   Vertex Data
 *     - v  Geometric vertices (not support w)
 *     - vt Texture vertices (not support w)
 *     - vn Vertex normals
 *
 *   Elements
 *     - f Face (only support triangulate faces)
 *
 *   Grouping
 *     - o Object name
 *
 *   Display
 *     - mtllib Material library (not support multiple files)
 *     - usemtl Material name
 *
 * MTL Supported List
 *
 *   Material Name
 *     - newmtl Material name
 *
 *   Texture Map
 *     - map_Kd Texture file is linked to the diffuse (not support options)
 */

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class FileReader
{

    public struct ObjectFile
    {
        public string o;
        public string mtllib;
        public List<string> usemtl;
        public List<Vector3> v;
        public List<Vector3> vn;
        public List<Vector2> vt;
        public List<List<int[]>> f;
        public List<Vector3> vc;
    }

    public struct MaterialFile
    {
        public List<string> newmtl;
        public List<string> mapKd;
    }

    public static ObjectFile ReadObjectFile(string path)
    {

        ObjectFile obj = new ObjectFile();
        string[] lines = File.ReadAllLines(path);

        obj.usemtl = new List<string>();
        obj.v = new List<Vector3>();
        obj.vn = new List<Vector3>();
        obj.vt = new List<Vector2>();
        obj.f = new List<List<int[]>>();
        obj.vc = new List<Vector3>();

        foreach (string line in lines)
        {
            if (line == "" || line.StartsWith("#"))
                continue;

            string[] token = line.Split(' ');
            switch (token[0])
            {

                case ("o"):
                    obj.o = token[1];
                    break;
                case ("mtllib"):
                    obj.mtllib = token[1];
                    break;
                case ("usemtl"):
                    obj.usemtl.Add(token[1]);
                    obj.f.Add(new List<int[]>());
                    break;
                case ("v"):
                    {
                        float token1;
                        if (!float.TryParse(token[1], out token1))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[1]}.");
                        }
                        float token2;
                        if (!float.TryParse(token[2], out token2))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[2]}.");
                        }
                        float token3;
                        if (!float.TryParse(token[3], out token3))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[3]}.");
                        }


                        obj.v.Add(new Vector3(
                            token1,
                            token2,
                            token3));

                        if (token.Length == 7)
                        {
                            float token4;
                            if (!float.TryParse(token[4], out token4))
                            {
                                Debug.Log($"The object file {token[0]} has an invalid value {token[4]}.");
                            }
                            float token5;
                            if (!float.TryParse(token[5], out token5))
                            {
                                Debug.Log($"The object file {token[0]} has an invalid value {token[5]}.");
                            }
                            float token6;
                            if (!float.TryParse(token[6], out token6))
                            {
                                Debug.Log($"The object file {token[0]} has an invalid value {token[6]}.");
                            }
                            obj.vc.Add(new Vector3(
                                token4,
                                token5,
                                token6));
                        }
                        break;
                    }
                case ("vn"):
                    {
                        float token1;
                        if (!float.TryParse(token[1], out token1))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[1]}.");
                        }
                        float token2;
                        if (!float.TryParse(token[2], out token2))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[2]}.");
                        }
                        float token3;
                        if (!float.TryParse(token[3], out token3))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[3]}.");
                        }

                        obj.vn.Add(new Vector3(
                            token1,
                            token2,
                            token3));
                        break;
                    }
                case ("vt"):
                    {
                        float token1;
                        if (!float.TryParse(token[1], out token1))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[1]}.");
                        }
                        float token2;
                        if (!float.TryParse(token[2], out token2))
                        {
                            Debug.Log($"The object file {token[0]} has an invalid value {token[2]}.");
                        }

                        obj.vt.Add(new Vector2(
                            token1,
                            token2));
                        break;
                    }
                case ("f"):
                    for (int i = 1; i < 4; ++i)
                    {
                        int[] triplet = Array.ConvertAll(token[i].Split('/'), x =>
                        {
                            if (String.IsNullOrEmpty(x))
                                return 0;
                            return int.Parse(x);
                        });

                        if (obj.f.Count == 0)
                        {
                            obj.f.Add(new List<int[]>());
                        }

                        obj.f[obj.f.Count - 1].Add(triplet);
                    }
                    break;
            }
        }

        return obj;
    }

    public static MaterialFile ReadMaterialFile(string path)
    {

        MaterialFile mtl = new MaterialFile();
        string[] lines = File.ReadAllLines(path);

        mtl.newmtl = new List<string>();
        mtl.mapKd = new List<string>();

        foreach (string line in lines)
        {
            if (line == "" || line.StartsWith("#"))
                continue;

            string[] token = line.Split(' ');
            switch (token[0])
            {

                case ("newmtl"):
                    mtl.newmtl.Add(token[1]);
                    break;
                case ("map_Kd"):
                    mtl.mapKd.Add(token[1]);
                    break;
            }
        }

        return mtl;
    }
}
