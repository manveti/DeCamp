using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCamp {
    public class Attribute {
        public enum Type { BOOL, INT, FLOAT, STRING };

        public Type type;
        public object value;

        public Attribute(Type type, object value) {
            this.type = type;
            this.value = value;
        }

        public virtual Attribute copy() {
            return new Attribute(this.type, this.value);
        }

        public virtual void adjust(object offset, bool inverse = false) {
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

    public class Character {
        public String player;
        public String name;
        protected Dictionary<String, Attribute> attributes;

        public virtual Character copy() {
            return new Character() {
                player = this.player,
                name = this.name,
                attributes = this.attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.copy())
            };
        }

        public virtual ICollection<String> getAttributes() {
            return this.attributes.Keys;
        }

        public virtual Attribute getRawAttribute(String key) {
            if (!this.attributes.ContainsKey(key)) { return null; }
            return this.attributes[key];
        }

        public virtual object getAttribute(String key) {
            Attribute attr = this.getRawAttribute(key);
            if (attr == null) { return null; }
            return attr.value;
        }

        public virtual void setRawAttribute(String key, Attribute attr) {
            if (attr == null) { this.clearAttribute(key); }
            else { this.attributes[key] = attr; }
        }

        public virtual void setAttribute(String key, object value, Attribute.Type type) {
            Attribute attr;
            if ((type == Attribute.Type.BOOL) && (!((bool)value))) { attr = null; }
            else { attr = new Attribute(type, value); }
            this.setRawAttribute(key, attr);
        }

        public virtual void clearAttribute(String key) {
            if (!this.attributes.ContainsKey(key)) { return; }
            this.attributes.Remove(key);
        }

        public virtual void adjustAttribute(String key, object offset, bool inverse = false) {
            if (!this.attributes.ContainsKey(key)) { return; }
            this.attributes[key].adjust(offset, inverse);
        }
    }
}
