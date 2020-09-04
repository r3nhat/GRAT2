import base64

class Encryption():
    
    def xor_crypt_string(self, data, k, encode=False, decode=False):
        if decode:
            data = base64.b64decode(data).decode('utf-8')
        key = str.encode(k)
        xored = ('').join((chr(ord(x) ^ 106) for x, y in zip(data, k)))
        if encode:
            nec = base64.b64encode(xored.encode()).strip()
            enc = nec.decode('utf-8')
            return enc
        return xored

    def RC4Crypt(self, data):
        # http://entitycrisis.blogspot.com/2011/04/encryption-between-python-and-c.html?m=1
        key = b'password'
        x = 0
        box = list(range(256))
        for i in range(256):
            x = (x + int(box[i]) + int(key[i % len(key)])) % 256
            box[i], box[x] = box[x], box[i]

        x = y = 0
        out = []

        for char in data:
            x = (x + 1) % 256
            y = (y + box[x]) % 256
            box[x], box[y] = box[y], box[x]
            out.append(char ^ box[(box[x] + box[y]) % 256])
        
        return b''.join([bytes([o]) for o in out])