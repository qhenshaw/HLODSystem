using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.HLODSystem.Streaming;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.HLODSystem.DebugWindow
{
    public class HLODItem : VisualElement
    {
        private static readonly string s_uxmlGuid = "a3d94d4fe01e43d4eb8f2fc24c533851";

        private HLODControllerBase m_controller;

        private Label m_lable;
        private Button m_ping;
        private ListView m_hierarchyView;
        public HLODItem()
        {
            var uxmlPath = AssetDatabase.GUIDToAssetPath(s_uxmlGuid);
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);

            var root = template.CloneTree();
            Add(root);

            m_lable = this.Q<Label>("Label");
            m_ping = this.Q<Button>("Ping");
            m_hierarchyView = this.Q<ListView>("Hierarchy");

            m_hierarchyView.makeItem += HierarchyMakeItem;
            m_hierarchyView.bindItem += HierarchyBindItem;

            
            m_ping.clickable.clicked += PingOnclicked;
        }
        public void BindController(HLODControllerBase controller)
        {
            m_controller = controller;
            
            this.Bind(new SerializedObject(controller));
            m_lable.Bind(new SerializedObject(controller.gameObject));

            List<HierarchyItemData> itemDatas = new List<HierarchyItemData>();
            Stack<HLODTreeNode> treeNodeTravelStack = new Stack<HLODTreeNode>();
            Stack<string> labelStack = new Stack<string>();
            
            treeNodeTravelStack.Push(m_controller.Root);
            labelStack.Push("");

            while (treeNodeTravelStack.Count > 0)
            {
                var node = treeNodeTravelStack.Pop();
                var label = labelStack.Pop();
                itemDatas.Add(new HierarchyItemData()
                {
                    Index = itemDatas.Count,
                    TreeNode = node,
                    Label = label,
                });
                
                for (int i = node.GetChildTreeNodeCount() - 1; i >= 0; --i)
                {
                    treeNodeTravelStack.Push(node.GetChildTreeNode(i));
                    labelStack.Push($"{label}_{i+1}");
                }
            }

            m_hierarchyView.itemsSource = itemDatas;
        }

        private VisualElement HierarchyMakeItem()
        {
            return new HierarchyItem(m_controller, m_hierarchyView);
        }
        private void HierarchyBindItem(VisualElement element, int i)
        {
            var data = m_hierarchyView.itemsSource[i] as HierarchyItemData;
            var item = element as HierarchyItem;

            if (item == null || data == null)
                return;

            data.Item = item;
            
            item.BindTreeNode(data);
        }
        
        private void PingOnclicked()
        {
            EditorGUIUtility.PingObject(m_controller);
        }
    }

}