using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;

public enum GemType
{
    None,
    White,
    Black,
    Red,
    Green,
    Blue,
}

// 赔率表

public class GemMulti
{
    public static double[,] mMultiTable_4 =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -99999},
        { 0, 0, 0, 0, 0.2, 0.4, 0.5, 0.8, 1, 2, 3, 5, 10, 20, 40, 40, -99999},  //1
        { 0, 0, 0, 0, 0.4, 0.5, 1, 2, 3, 5, 10, 25, 50, 75, 80, 80, -99999}, //2
        { 0, 0, 0, 0, 0.5, 1, 2, 4, 8, 16, 50, 100, 200, 500, 600, 600, -99999}, //3
        { 0, 0, 0, 0, 1, 3, 5, 6, 10, 75, 100, 1000, 2000, 5000, 6000, 6000, -99999}, //4
        { 0, 0, 0, 0, 2, 5, 10, 50, 100, 200, 500, 2000, 5000, 6000, 8000, 8000, -99999}, //5
    };

    public static double[,] mMultiTable_5 =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -99999},
        { 0, 0, 0, 0, 0, 0.2, 0.4, 0.5, 0.8, 1, 2, 3, 5, 10, 20, 45, 45, 45, 45, 45, -99999, -99999, -99999, -99999, -99999, -99999},  //1
        { 0, 0, 0, 0, 0, 0.4, 0.5, 1, 2, 3, 5, 10, 25, 50, 75, 100, 100, 100, 100, 100, -99999, -99999, -99999, -99999, -99999, -99999}, //2
        { 0, 0, 0, 0, 0, 0.5, 1, 2, 4, 8, 16, 50, 100, 200, 500, 700, 700, 700, 700, 700, -99999, -99999, -99999, -99999, -99999, -99999}, //3
        { 0, 0, 0, 0, 0, 1, 3, 5, 6, 10, 75, 100, 1000, 2000, 5000, 7000, 7000, 7000, 7000, 7000, -99999, -99999, -99999, -99999, -99999, -99999}, //4
        { 0, 0, 0, 0, 0, 2, 5, 10, 50, 100, 200, 500, 2000, 5000, 8000, 10000, 10000, 10000, 10000, 10000, -99999, -99999, -99999, -99999, -99999, -99999}, //5
    };

    public static double[,] mMultiTable_6 =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 , 0, 0, 0 , 0, 0, 0, 0, 0, 0, -99999},
        { 0, 0, 0, 0, 0, 0, 0.2, 0.4, 0.5, 0.8, 1, 2, 3, 5, 10, 20, 50, 50, 50, 50, 50, 50, 50, 50, -99999, -99999, -99999, -99999, -99999, -99999, -99999, -99999, -99999, -99999, -99999, -99999, -99999},  //1
        { 0, 0, 0, 0, 0, 0, 0.4, 0.5, 1, 2, 3, 5, 10, 25, 50, 75, 120, 120, 120, 120, 120, 120, 120, 120, -99999, -99999, -99999, -99999, -99999, -99999, -99999,-99999, -99999, -99999, -99999, -99999, -99999}, //2
        { 0, 0, 0, 0, 0, 0, 0.5, 1, 2, 4, 8, 16, 50, 100, 200, 500, 800, 800, 800, 800, 800, 800, 800, 800, -99999, -99999, -99999, -99999, -99999, -99999, -99999,-99999, -99999, -99999, -99999, -99999, -99999}, //3
        { 0, 0, 0, 0, 0, 0, 1, 3, 5, 6, 10, 75, 100, 1000, 2000, 5000, 8000, 8000, 8000, 8000, 8000, 8000, 8000, 8000, -99999, -99999, -99999, -99999, -99999, -99999, -99999,-99999, -99999, -99999, -99999, -99999, -99999}, //4
        { 0, 0, 0, 0, 0, 0, 2, 5, 10, 50, 100, 200, 500, 2000, 5000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, -99999, -99999, -99999, -99999, -99999, -99999, -99999,-99999, -99999, -99999, -99999, -99999, -99999}, //5
    };
}


// 区间的桶
public class GemBetween
{
    const double test = 1000;
    public static double[,] mBetween_4 =
    {
        {0, 0.4, 0},
        {0.4, 1, 300000/test},
        {1, 2, 400000/test},
        {2, 4, 200000/test},
        {4, 6, 100000/test},
        {6, 8, 100000/test},
        {8, 10, 80000/test},
        {10, 12, 60000/test},
        {12, 15, 50000/test},
        {15, 20, 40000/test},
        {20, 25, 30000/test},
        {25, 50, 16000/test},
        {50, 55, 15000/test},
        {55, 60, 12000/test},
        {60, 75, 10000/test},
        {75, 80, 10000/test},
        {80, 90, 9000/test},
        {90, 100, 8000/test},
        {100, 105, 7000/test},
        {105, 110, 7000/test},
        {110, 120, 6000/test},
        {120, 150, 5000/test},
        {150, 200, 4000/test},
        {200, 1000, 3000/test},
        {1000, 2000, 2000/test},
        {2000, 5000, 1000/test},
        {5000, 8000, 1000/test},
        {8000, 80000, 1000/test},
    };

    public static double[,] mBetween_5 =
    {
        {0, 0.4, 0},
        {0.4, 1, 300000/test},
        {1, 2, 400000/test},
        {2, 4, 200000/test},
        {4, 6, 100000/test},
        {6, 8, 100000/test},
        {8, 10, 80000/test},
        {10, 12, 60000/test},
        {12, 15, 50000/test},
        {15, 20, 40000/test},
        {20, 25, 30000/test},
        {25, 30, 16000/test},
        {30, 50, 15000/test},
        {50, 55, 15000/test},
        {55, 60, 12000/test},
        {60, 75, 10000/test},
        {75, 80, 10000/test},
        {80, 90, 9000/test},
        {90, 100, 8000/test},
        {100, 105, 7000/test},
        {105, 110, 7000/test},
        {110, 120, 6000/test},
        {120, 150, 5000/test},
        {150, 200, 4000/test},
        {200, 300, 3000/test},
        {300, 1000, 3000/test},
        {1000, 2000, 3000/test},
        {2000, 5000, 2000/test},
        {5000, 7000, 2000/test},
        {7000, 8000, 1000/test},
        {8000, 10000, 1000/test},
        {10000, 100000, 1000/test},
    };

    public static double[,] mBetween_6 =
    {
        {0, 0.4, 0},
        {0.4, 1, 300000/test},
        {1, 2, 400000/test},
        {2, 4, 200000/test},
        {4, 6, 100000/test},
        {6, 8, 100000/test},
        {8, 10, 80000/test},
        {10, 12, 60000/test},
        {12, 15, 50000/test},
        {15, 20, 40000/test},
        {20, 25, 30000/test},
        {25, 30, 16000/test},
        {30, 50, 15000/test},
        {50, 55, 15000/test},
        {55, 60, 12000/test},
        {60, 75, 10000/test},
        {75, 80, 10000/test},
        {80, 90, 9000/test},
        {90, 100, 8000/test},
        {100, 105, 7000/test},
        {105, 110, 7000/test},
        {110, 120, 6000/test},
        {120, 150, 5000/test},
        {150, 200, 4000/test},
        {200, 300, 3000/test},
        {300, 1000, 3000/test},
        {1000, 2000, 3000/test},
        {2000, 5000, 2000/test},
        {5000, 7000, 2000/test},
        {7000, 8000, 1000/test},
        {8000, 10000, 1000/test},
        {10000, 100000, 1000/test},
    };
}

public class GemInfo
{
    public GemType type;
    public bool b_visit = false;
    public bool b_isline = false;
}

// 一盘游戏的数据
public class TableInfo
{
    public double multi = 0;
    public int line_count = 0;
    public string table_info = "";
}


public class GemGenerate
{
    List<List<GemInfo>> m_Table = new List<List<GemInfo>>();
    List<List<GemType>> m_drop_table = new List<List<GemType>>();

    public int m_x, m_y;

    public int[] m_count = new int[128];
    public int[] m_line_count = new int[128];
    public int[] m_between_count = new int[128];
    public double[] m_average_multi = new double[128];
    public List<TableInfo>[] m_cons_list = new List<TableInfo>[128];

    public int total_add = 0;

    public double[,] mBetween;
    public double[,] mMultiTable;

    public int need_len = 4;
    public double GetMulti(GemType type, int count)
    {
        return mMultiTable[(int)type, count];
    }

    public int GetBetween(double multi)
    {
        int count = mBetween.Length / 3;
        for (int i = 0; i < count; i++)
        {
            if (multi < mBetween[i, 1])
            {
                return i;
            }
        }
        return count;
    }

    // m_x必须等于 m_y 否则会出问题
    public void GetOne()
    {
        m_drop_table.Clear();
        m_Table.Clear();
        for (int i = 0; i < m_x; i++)
        {
            List<GemInfo> temp = new List<GemInfo>();
            List<GemType> type_temp = new List<GemType>();
            for (int j = 0; j < m_y; j++)
            {
                GemInfo _gem_info = new GemInfo();
                _gem_info.type = (GemType)Random.Range(1, 6);

                temp.Add(_gem_info);
            }
            m_drop_table.Add(type_temp);
            m_Table.Add(temp);
        }
    }

    public string TableToString()
    {
        StringBuilder sb = new StringBuilder(100);
        for (int i = 0; i < m_x; i++)
        {
            for (int j = 0; j < m_y; j++)
            {
                sb.Append((char)((int)m_Table[i][j].type + '0'));
            }
        }
        return sb.ToString();
    }

    public string DropToString()
    {
        StringBuilder sb = new StringBuilder(256);
        int count = 0;
        while (count < 70)
        {
            bool has_left = false;
            for (int i = 0; i < m_x; i++)
            {
                if (m_drop_table[i].Count > count)
                {
                    sb.Append((char)((int)m_drop_table[i][count] + '0'));
                    has_left = true;
                }
                else
                {
                    sb.Append('0');
                }
            }
            count += 1;
            if (!has_left)
            {
                break;
            }
        }
        return sb.ToString();
    }

    public struct XYPoint
    {
        public int x, y;
        public XYPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    void CheckPoint(Stack<XYPoint> point_stack, int cur_x, int cur_y, GemType dfs_type)
    {
        if (!(cur_x < 0 || cur_y < 0 || cur_x >= m_x || cur_y >= m_y || m_Table[cur_x][cur_y].b_visit)
            && m_Table[cur_x][cur_y].type == dfs_type)
        {
            point_stack.Push(new XYPoint(cur_x, cur_y));
            m_Table[cur_x][cur_y].b_visit = true;
        }
    }

    int dfs(int cur_x ,int cur_y, GemType dfs_type)
    {
        Stack<XYPoint> point_stack = new Stack<XYPoint>();
        point_stack.Push(new XYPoint(cur_x, cur_y));
        m_Table[cur_x][cur_y].b_visit = true;

        List<XYPoint> visit_list = new List<XYPoint>();

        while (point_stack.Count > 0)
        {
            XYPoint temp = point_stack.Pop();
            visit_list.Add(temp);
            // 找四个方向
            CheckPoint(point_stack, temp.x + 1, temp.y, dfs_type);
            CheckPoint(point_stack, temp.x - 1, temp.y, dfs_type);
            CheckPoint(point_stack, temp.x, temp.y + 1, dfs_type);
            CheckPoint(point_stack, temp.x, temp.y - 1, dfs_type);
        }

        if (visit_list.Count >= need_len)
        {
            foreach (var point in visit_list)
            {
                m_Table[point.x][point.y].b_isline = true;
            }
            m_line_count[visit_list.Count] += 1;
        }
        return visit_list.Count;
    }

    public void Simulate(int count, double multi, string table_str)
    {

        for (int i = 0; i < m_x; i++)
        {
            for (int j = 0; j < m_y; j++)
            {
                m_Table[i][j].b_visit = false;
                m_Table[i][j].b_isline = false;
            }
        }
        bool has_line = false;
        // 做一次dfs找出可以消掉的组合
        for (int i = 0; i < m_x; i++)
        {
            for (int j = 0; j < m_y; j++)
            {
                if (!m_Table[i][j].b_visit)
                {
                    int multi_count = dfs(i, j, m_Table[i][j].type);
                    if (multi_count >= need_len)
                    {
                        // 叠加倍数上去
                        multi += GetMulti(m_Table[i][j].type, multi_count);
                        has_line = true;
                        count += 1;
                    }
                }
            }
        }

        int[] color_temp = new int[6];
        // 找出剩余方块里最多的颜色。以便提高其生成概率
        for (int i = 0; i < m_x; i++)
        {
            for (int j = 0; j < m_y; j++)
            {
                if (!m_Table[i][j].b_isline)
                {
                    color_temp[(int)m_Table[i][j].type] += 1;
                }
            }
        }

        int max_color = 0;
        GemType max_type = GemType.White;
        for (int i = 1; i <= 5; i++)
        {
            if (color_temp[i] > max_color)
            {
                max_color = color_temp[i];
                max_type = (GemType)i;
            }
        }

        // 向下移动，
        int[] find = {0, 0, 0, 0, 0, 0};
        for (int i = 0; i < m_x; i++)
        {
            for (int j = 0; j < m_y; j++)
            {
                if (m_Table[i][j].b_isline)
                {
                    if (find[j] <= i)
                    {
                        find[j] = i + 1;
                    }
                    while (find[j] < m_x)
                    {
                        if (!m_Table[find[j]][j].b_isline)
                        {
                            m_Table[i][j].type = m_Table[find[j]][j].type;
                            m_Table[i][j].b_isline = false;
                            m_Table[find[j]][j].b_isline = true;
                            break;
                        }
                        find[j] += 1;
                    }

                    // 填充颜色，其实就是掉下来的
                    if (find[j] >= m_x)
                    {
                        int key = Random.Range(1, 10);
                        if (key > 5)
                        {
                            m_Table[i][j].type = max_type;
                        }
                        else
                        {
                            m_Table[i][j].type = (GemType)key;
                        }
                        m_drop_table[j].Add(m_Table[i][j].type);
                    }
                }
            }
        }

        //再来一次, 15 连就不管了，免得爆了
        if (has_line && count < 15)
        {
            Simulate(count, multi, table_str);
        }
        else
        {
            m_count[count] += 1;
            // 只记录多消
            if (count >= 2 && count <= 10 && multi > 0)
            {
                TableInfo info = new TableInfo();
                info.multi = multi;
                info.line_count = count;
                info.table_info = table_str + DropToString();

                int between_index = GetBetween(multi);
                if (m_cons_list[between_index] == null)
                {
                    m_cons_list[between_index] = new List<TableInfo>();
                }
                // 加进去  当需要看全部的时候注掉条件
                if (m_cons_list[between_index].Count < mBetween[between_index, 2] - 0.5)
                {
                    m_cons_list[between_index].Add(info);
                    m_average_multi[between_index] += multi;
                    m_between_count[between_index] += 1;
                    total_add += 1;
                }
            }
        }
    }

    // 先关闭入口，防止手抖覆盖
    [MenuItem("LuaFramework/Gen Gem Table ALL")]
    public static void GenGenTable_ALL()
    {
        if (Directory.Exists(Application.dataPath + "/LineGen"))
        {
            Directory.Delete(Application.dataPath + "/LineGen", true);
        }
        Directory.CreateDirectory(Application.dataPath + "/LineGen");
        Directory.CreateDirectory(Application.dataPath + "/LineGen/4");
        Directory.CreateDirectory(Application.dataPath + "/LineGen/5");
        Directory.CreateDirectory(Application.dataPath + "/LineGen/6");

        GenGenTable_4();
        GenGenTable_5();
        GenGenTable_6();

        AssetDatabase.Refresh();
    }

    //[MenuItem("LuaFramework/Gen Gem Table 4")]
    public static void GenGenTable_4()
    {
        GemGenerate test = new GemGenerate();
        test.need_len = test.m_x = test.m_y = 4;

        test.mBetween = GemBetween.mBetween_4;
        test.mMultiTable = GemMulti.mMultiTable_4;

        TestGemTable(test);
    }

    //[MenuItem("LuaFramework/Gen Gem Table 5")]
    public static void GenGenTable_5()
    {
        GemGenerate test = new GemGenerate();
        test.need_len = test.m_x = test.m_y = 5;

        test.mBetween = GemBetween.mBetween_5;
        test.mMultiTable = GemMulti.mMultiTable_5;

        TestGemTable(test);
    }

    //[MenuItem("LuaFramework/Gen Gem Table 6")]
    public static void GenGenTable_6()
    {
        GemGenerate test = new GemGenerate();
        test.need_len = test.m_x = test.m_y = 6;

        test.mBetween = GemBetween.mBetween_6;
        test.mMultiTable = GemMulti.mMultiTable_6;

        TestGemTable(test);
    }

    public static void TestGemTable(GemGenerate test)
    {

        System.DateTime times = System.DateTime.Now;

        double to_add = 0;
        for (int i = 0; i < test.mBetween.Length / 3; i++)
        {
            to_add += test.mBetween[i, 2];
        }
        to_add -= 0.5;
        while (test.total_add < to_add)
        {
            test.GetOne();
            test.Simulate(0, 0, test.TableToString());
        }

        string cons = "";
        for (int i = 0; i < 25; i++)
        {
            cons += test.m_count[i] + " ";
        }
        Debug.LogError(cons);

        cons = "";
        for (int i = 0; i <= test.m_x * test.m_x; i++)
        {
            cons += test.m_line_count[i] + " ";
        }
        Debug.LogError(cons);

        cons = "";
        for (int i = 0; i < test.mBetween.Length / 3; i++)
        {
            cons += test.m_between_count[i] + " ";
        }
        Debug.LogError(cons);

        cons = "";
        for (int i = 0; i < test.mBetween.Length / 3; i++)
        {
            cons += test.m_average_multi[i] / (test.m_between_count[i] == 0 ? 1: test.m_between_count[i]) + " ";
        }
        Debug.LogError(cons);
        Debug.LogError((System.DateTime.Now - times).TotalSeconds);

        StringBuilder sb = new StringBuilder();

        // 导出样本表
        for (int i = 1; i < test.mBetween.Length / 3; i++)
        {
            int sb_lines = 0;
            int total_subfiles = 1;
            sb.Clear();
            if (test.m_cons_list[i] != null)
            {
                sb.AppendLine("local DB_SUB = {");
                foreach (TableInfo info in test.m_cons_list[i])
                {
                    sb.AppendLine("\t{multi= " + info.multi + ",line_count=" + info.line_count + ",table_info= \"" + info.table_info + "\" }, ");
                    sb_lines++;
                    if (sb_lines > 60000)
                    {
                        sb.AppendLine("}");
                        sb.AppendLine("return DB_SUB");
                        File.WriteAllText("Assets/LineGen/" + test.m_y + "/line_cons_" + test.m_y + "_" + i + "_" + total_subfiles + ".lua", sb.ToString(), new UTF8Encoding(false));
                        sb.Clear();
                        sb.AppendLine("local DB_SUB = {");
                        sb_lines = 0;
                        total_subfiles++;
                    }
                }
                sb.AppendLine("}");
                sb.AppendLine("return DB_SUB");
                File.WriteAllText("Assets/LineGen/" + test.m_y + "/line_cons_" + test.m_y + "_" + i + "_" + total_subfiles + ".lua", sb.ToString(), new UTF8Encoding(false));
            }



            sb.Clear();
            sb.AppendLine("DB_" + test.m_y + "_" + i + " = {}");
            sb.AppendLine("local _temp");
            for (int _x = 1; _x <= total_subfiles; _x++)
            {
                sb.AppendLine("_temp = require \"LineGen/" + test.m_y + "/line_cons_" + test.m_y + "_" + i + "_" + _x + "\"");
                sb.AppendLine("for k,v in pairs(_temp) do");
                sb.AppendLine("\ttable.insert(DB_" + test.m_y + "_" + i + ", v)");
                sb.AppendLine("end");
            }
            File.WriteAllText("Assets/LineGen/" + test.m_y + "/line_cons_" + test.m_y + "_" + i + ".lua", sb.ToString(), new UTF8Encoding(false));

        }

        // 导出总表
        sb.Clear();
        sb.AppendLine("DB_TOTAL_" + test.m_y + " = {");
        for (int i = 1; i < test.mBetween.Length / 3; i++)
        {
            sb.AppendLine("\t{average=" + test.m_average_multi[i] / (test.m_between_count[i] == 0 ? 1 : test.m_between_count[i]) + ",rate= 1" + ",count=" + test.m_between_count[i] + "}, ");
        }
        sb.AppendLine("}");
        sb.AppendLine("local total_rate = 0");
        sb.AppendLine("for i = 1, #DB_TOTAL_" + test.m_y + " do");
        sb.AppendLine("\trequire(\"LineGen/" + test.m_y + "/line_cons_" + test.m_y + "_\"..i)");
        sb.AppendLine("\ttotal_rate = total_rate + DB_TOTAL_" + test.m_y + "[i].rate");
        sb.AppendLine("end");
        sb.AppendLine("local random = math.random");
        sb.AppendLine("function GetLineSample" + test.m_y + "()");
        sb.AppendLine("\tlocal rand = random(total_rate)");
        sb.AppendLine("\tfor i = 1, #DB_TOTAL_" + test.m_y + " do");
        sb.AppendLine("\t\tif rand <= DB_TOTAL_" + test.m_y + "[i].rate then");
        sb.AppendLine("\t\t\treturn _G['DB_" + test.m_y + "_'..i][random(DB_TOTAL_" + test.m_y + "[i].count)]");
        sb.AppendLine("\t\telse");
        sb.AppendLine("\t\t\trand = rand - DB_TOTAL_" + test.m_y + "[i].rate");
        sb.AppendLine("\t\tend");
        sb.AppendLine("\tend");
        sb.AppendLine("end");
    
        File.WriteAllText("Assets/LineGen/" + test.m_y + "/line_total_" + test.m_y + ".lua", sb.ToString(), new UTF8Encoding(false));
    }
}