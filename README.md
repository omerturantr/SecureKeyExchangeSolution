# 🔐 SecureKeyExchangeSolution

**Course:** Computer and Network Security (BIM 437)  
**Project Type:** Term Project  
**Technology:** C# / .NET 8 / WinForms  

---

## 📌 Project Overview

SecureKeyExchangeSolution is a demo-level secure communication project that simulates:

- A **Certificate Authority (CA)**
- Two clients (**Client1** and **Client2**)
- Secure key exchange using **RSA** and **Session Keys (Ks)**

The project demonstrates:
- Public Key Infrastructure (PKI)
- Certificate-based authentication
- Secure session key transfer between clients

---

## 🏗 Project Structure

SecureKeyExchangeSolution
│
├── CAApp # Certificate Authority application
├── Client1App # Client that initiates communication
├── Client2App # Client that listens and receives keys
├── SharedSecurityLib # Shared crypto, protocol, and model classes
└── SecureKeyExchangeSolution.sln


---

## 🔐 Cryptographic Features

- RSA 2048-bit key generation
- CA-signed client certificates
- Certificate verification using CA public key
- Secure session key (Ks) generation and transfer
- Encrypted communication over TCP sockets

---

## 📡 Network Configuration

| Component | IP Address | Port |
|---------|-----------|------|
| CA Server | 127.0.0.1 | 9000 |
| Client2 Listener | 127.0.0.1 | 9100 |

---

## ▶️ How to Run the Project (Step-by-Step)

### 1️⃣ Start CAApp
- Click **Generate CA Keys**
- Click **Start CA Server**
- Verify that the server starts on `127.0.0.1:9000`

### 2️⃣ Start Client2App
- Click **Connect to CA**
- Verify certificate validation
- Click **Start Listener**
- Listener starts on `127.0.0.1:9100`

### 3️⃣ Start Client1App
- Click **Connect to CA**
- Verify certificate validation
- Click **Send Ks to Client2**

✅ Client2 successfully decrypts the session key  
✅ Secure communication established

---

## 🧪 Demonstrated Protocol Messages

- `REQ_CERT`
- `CERT`
- `GET_PUBLIC_KEY`
- `PEER_CERT`
- `SESSION_KEY`
- `SESSION_CONFIRM`

---

## ✅ Project Status

✔ CA implemented  
✔ Client authentication via certificates  
✔ Secure session key exchange  
✔ No build errors  

🟢 **Project Completed Successfully**

---

## 📚 Educational Purpose

This project is developed for **educational purposes** to demonstrate:
- PKI fundamentals
- Secure key exchange
- Network security principles

It is **not intended for production use**.

---

## 👤 Author

**Ömer Faruk Turan**  
Computer Engineering Student  
GitHub: https://github.com/omerturantr

---

## 📄 License

This project is shared for academic use only.
