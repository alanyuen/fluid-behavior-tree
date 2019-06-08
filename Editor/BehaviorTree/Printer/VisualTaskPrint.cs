using System.Linq;
using CleverCrow.Fluid.BTs.TaskParents;
using UnityEditor;
using UnityEngine;

namespace CleverCrow.Fluid.BTs.Trees.Editors {
    public class VisualTaskPrint {
        private readonly VisualTask _node;
        private readonly ColorFader _backgroundFader = new ColorFader(new Color(0.65f, 0.65f, 0.65f), new Color(0.39f, 0.78f, 0.39f));
        private readonly ColorFader _textFader = new ColorFader(Color.white, Color.black);
        private bool _activatedOnce;
        
        private readonly IGraphBox _box;
        private readonly IGraphBox _divider;

        private GUIStyle _boxStyle;
        private Texture2D _boxBorder;
        private GUIStyle _boxStyleInactive;
        private Texture2D _boxBorderInactive;
        private Texture2D _dividerGraphic;
        private Texture2D _verticalBottom;
        private Texture2D _verticalTop;
        private Texture2D _iconTexture;

        public VisualTaskPrint (VisualTask node) {
            _node = node;
            _box = node.Box;
            _divider = node.Divider;
            
            _iconTexture = AssetDatabase.LoadAssetAtPath<Sprite>(_node.Task.IconPath)?.texture;
            
            CreateBoxStyles();
        }
        
        public void Print (bool taskIsActive) {
            if (!(_node.Task is TaskRoot)) PaintVerticalTop();
            if (taskIsActive) _activatedOnce = true;
            
            _backgroundFader.Update(taskIsActive);
            _textFader.Update(taskIsActive);
            PaintBody();
            
            if (_node.Children.Count > 0) {
                PaintDivider();
                PaintVerticalBottom();
            }
        }

        private void CreateBoxStyles () {
            const int fontSize = 9;
            
            _boxBorder = CreateTexture(19, 19, Color.gray);
            _boxBorder.SetPixels(1, 1, 17, 17, 
                Enumerable.Repeat(Color.white, 17 * 17).ToArray());
            _boxBorder.Apply();
            _boxStyle = new GUIStyle(GUI.skin.box) {
                border = new RectOffset(1, 1, 1, 1),
                fontSize = fontSize,
                alignment = TextAnchor.LowerCenter,
                normal = {
                    background = _boxBorder,
                },
            };

            Color inactiveColor = new Color32(208, 208, 208, 255);
            _boxBorderInactive = CreateTexture(19, 19, new Color32(150, 150, 150, 255));
            _boxBorderInactive.SetPixels(1, 1, 17, 17, 
                Enumerable.Repeat(inactiveColor, 17 * 17).ToArray());
            _boxBorderInactive.Apply();
            _boxStyleInactive = new GUIStyle(GUI.skin.box) {
                border = new RectOffset(1, 1, 1, 1),
                fontSize = 9,
                alignment = TextAnchor.LowerCenter,
                normal = {
                    background = _boxBorderInactive,
                    textColor = Color.gray,
                },
            };
        }

        private void PaintBody () {
            var rect = new Rect(
                _box.GlobalPositionX + _box.PaddingX, 
                _box.GlobalPositionY + _box.PaddingY,
                _box.Width - _box.PaddingX, 
                _box.Height - _box.PaddingY);

            var prevBackgroundColor = GUI.backgroundColor;
            var prevColor = GUI.color;

            if (_activatedOnce) {
                GUI.backgroundColor = _backgroundFader.CurrentColor;
                _boxStyle.normal.textColor = _textFader.CurrentColor;
                GUI.Box(rect, _node.Task.Name, _boxStyle);
            } else {
                GUI.Box(rect, _node.Task.Name, _boxStyleInactive);
            }

            PrintIcon();

            GUI.backgroundColor = prevBackgroundColor;
            GUI.color = prevColor;
        }

        private void PrintIcon () {
            const float iconWidth = 35;
            const float iconHeight = 35;
            var iconRect = new Rect(
                _box.GlobalPositionX + _box.PaddingX / 2 + _box.Width / 2 - iconWidth / 2 + _node.Task.IconPadding / 2,
                _box.GlobalPositionY + _box.PaddingX / 2 + 3 + _node.Task.IconPadding / 2,
                iconWidth - _node.Task.IconPadding,
                iconHeight - _node.Task.IconPadding);
            GUI.Label(iconRect, _iconTexture);
        }

        private void PaintDivider () {
            const int graphicSizeIncrease = 5;
            
            if (_dividerGraphic == null) {
                _dividerGraphic = CreateTexture(
                    (int)_divider.Width + graphicSizeIncrease, 
                    1, 
                    Color.black);
            }

            var position = new Rect(
                _divider.GlobalPositionX + _box.PaddingY / 2 + _node.DividerLeftOffset - 2, 
                // @TODO Should not need to offset this
                _divider.GlobalPositionY + _box.PaddingY / 2,
                _divider.Width + graphicSizeIncrease, 
                10);
            
            GUI.Label(position, _dividerGraphic);
        }

        private void PaintVerticalBottom () {
            if (_verticalBottom == null) {
                _verticalBottom = CreateTexture(1, (int)_box.PaddingY, Color.black);
            }

            var position = new Rect(
                _box.GlobalPositionX + _node.Width / 2 + _box.PaddingX - 2, 
                _box.GlobalPositionY + _node.Height + _box.PaddingY - 1,
                100, 
                _box.PaddingY - 1);
            
            GUI.Label(position, _verticalBottom);
        }
        
        private void PaintVerticalTop () {
            if (_verticalTop == null) {
                _verticalTop = CreateTexture(1, Mathf.RoundToInt(_box.PaddingY / 2), Color.black);
            }

            var position = new Rect(
                _box.GlobalPositionX + _node.Width / 2 + _box.PaddingX - 2, 
                _box.GlobalPositionY + _box.PaddingY / 2,
                100, 
                10);
            
            GUI.Label(position, _verticalTop);
        }
        
        private static Texture2D CreateTexture (int width, int height, Color color) {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            texture.SetPixels(Enumerable.Repeat(color, width * height).ToArray());
            texture.Apply();
            
            return texture;
        }
    }
}
