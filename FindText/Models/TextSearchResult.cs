using System;

namespace FindText.Models
{
    public class TextSearchResult : VModelsBase
    {
        string _filePath;
        string _title;
        double _size;
        string _preview;
        int _position;
        DateTime? _lastData;
        string _note;
        bool _isSelected;

        public TextSearchResult()
        {
            _isSelected = false;
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
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
                RaisePropertyChanged();
            }

        }
        

        public bool IsSelected
        {
            get
            {
                return this._isSelected;
            }
            set
            {
                this._isSelected = value;
                RaisePropertyChanged();
            }

        }

    }
}