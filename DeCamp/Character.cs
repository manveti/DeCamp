using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    class Attribute {
        public enum Type { BOOL, INT, FLOAT, STRING };

        public Type type;
        public object value;

        public Attribute(Type type, object value) {
            this.type = type;
            this.value = value;
        }

        public void adjust(object offset, bool inverse = false) {
            switch (this.type) {
            case Type.BOOL:
                throw new ArgumentException("Cannot adjust boolean attribute");
            case Type.INT:
                if (inverse) { this.value = ((int)(this.value)) - ((int)offset); }
                else { this.value = ((int)(this.value)) + ((int)offset); }
                break;
            case Type.FLOAT:
                if (inverse) { this.value = ((double)(this.value)) - ((double)offset); }
                else { this.value = ((double)(this.value)) + ((double)offset); }
                break;
            case Type.STRING:
                if (inverse) {
                    int idx = ((String)(this.value)).Length - ((String)offset).Length;
                    if ((String)offset != ((String)(this.value)).Substring(idx)) {
                        String errMsg = String.Format("Cannot trim string '{0}': its tail is not '{1}'", (String)(this.value), (String)offset);
                        throw new ArgumentException(errMsg);
                    }
                    this.value = ((String)(this.value)).Substring(0, idx);
                }
                else { this.value = ((String)(this.value)) + ((String)offset); }
                break;
            }
        }
    }

    class Character {
        public String player;
        public String name;
        protected Dictionary<String, Attribute> attributes;
        protected SortedSet<Timestamp> modEventTimes;

        public ICollection<String> getAttributes() {
            return this.attributes.Keys;
        }

        public Attribute getRawAttribute(String key) {
            if (!this.attributes.ContainsKey(key)) { return null; }
            return this.attributes[key];
        }

        public object getAttribute(String key) {
            Attribute attr = this.getRawAttribute(key);
            if (attr == null) { return null; }
            return attr.value;
        }

        public void setRawAttribute(String key, Attribute attr) {
            if (attr == null) { this.clearAttribute(key); }
            else { this.attributes[key] = attr; }
        }

        public void setAttribute(String key, object value, Attribute.Type type) {
            Attribute attr;
            if ((type == Attribute.Type.BOOL) && (!((bool)value))) { attr = null; }
            else { attr = new Attribute(type, value); }
            this.setRawAttribute(key, attr);
        }

        public void clearAttribute(String key) {
            if (!this.attributes.ContainsKey(key)) { return; }
            this.attributes.Remove(key);
        }

        public void adjustAttribute(String key, object offset, bool inverse = false) {
            if (!this.attributes.ContainsKey(key)) { return; }
            this.attributes[key].adjust(offset, inverse);
        }
    }
}
