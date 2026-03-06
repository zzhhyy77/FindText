using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindText.Models
{
    public class SelectedFile : VModelsBase
    {

        string _filePath;
        string _title;
        double _size;
        string _preview;
        int _position;
        DateTime? _lastData;
        string _note;


        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                this.RaisePropertyChanged();
            }
        }

        public string FilePath
        {
            get
            {
                return this._filePath;
            }
            set
            {
                this._filePath = value;
                this.RaisePropertyChanged();
            }
        }


        public double Size
        {
            get
            {
                return this._size;
            }
            set
            {
                this._size = value;
                this.RaisePropertyChanged();
            }
        }



        public DateTime? LastDate
        {
            get
            {
                return this._lastData;
            }
            set
            {
                this._lastData = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 文件内容的预览
        /// </summary>
        public string Preview
        {
            get
            {
                return this._preview;
            }
            set
            {
                this._preview = value;
                this.RaisePropertyChanged();
            }
        }

        public int Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
                this.RaisePropertyChanged();
            }
        }

        /// <summary>
        /// 手工添加的备注信息，方便用户记录一些额外的信息
        /// </summary>
        public string Note
        {
            get
            {
                return this._note;
            }
            set
            {
                this._note = value;
                this.RaisePropertyChanged();
            }

        }
    }
}
