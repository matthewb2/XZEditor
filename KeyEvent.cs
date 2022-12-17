using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XZ.Edit.Actions;

namespace XZ.Edit {
    public class KeyEvent {

        private Dictionary<Keys, BaseAction> keyEvents = new Dictionary<Keys, BaseAction>();
        private Parser pParser;
        public KeyEvent(Parser parser) {
            this.pParser = parser;
            keyEvents[Keys.Left] = new LeftAction(parser);
            keyEvents[Keys.Right] = new RightAction(parser);
            keyEvents[Keys.Up] = new UPAction(parser);
            keyEvents[Keys.Down] = new DownAction(parser);            
            keyEvents[Keys.Control | Keys.A] = new AllSelectAction(parser);            
            keyEvents[Keys.Control | Keys.C] = new CopyAction(parser);            
            keyEvents[Keys.Control | Keys.Z] = new UndoAction(parser);
            keyEvents[Keys.Control | Keys.Y] = new RedoAction(parser);
            keyEvents[Keys.Shift | Keys.Left] = new SelectLeftAction(parser);
            keyEvents[Keys.Shift | Keys.Right] = new SelectRightAction(parser);
            keyEvents[Keys.Shift | Keys.Down] = new SelectDownAction(parser);
            keyEvents[Keys.Shift | Keys.Up] = new SelectUPAction(parser);
            keyEvents[Keys.Shift | Keys.End] = new EndAction(parser);
            
        }

        public BaseAction GetKeyEvent(Keys k) {
            BaseAction action = null;
            if (keyEvents.ContainsKey(k)) {
                action =  keyEvents[k];
                action.PIsAddUndo = false;
                return action;
            } else {

                switch (k) {
                    case Keys.Delete:
                        action = new DeleteAction(this.pParser);
                        break;
                    case Keys.Control | Keys.X:
                        action = new CutAction(this.pParser);
                        break;
                    case Keys.Control | Keys.V:
                        action = new PasteAction(this.pParser);
                        action.PIsUndoOrRedo = false;
                        break;
                }
                return action;
            }
        }

        public static Keys UpDonw = Keys.Up | Keys.Down;

        /// <summary>
        /// 是否包含K
        /// </summary>
        /// <param name="ks"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool Contains(Keys ks, Keys k) {
            return ((ks & k) == k);
        }
    }
}
