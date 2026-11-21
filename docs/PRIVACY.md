# Privacy / ความเป็นส่วนตัว

## English

- The app's live input is the explicitly saved capture device, or the recognized Live Gamer MINI on first use. It does not guess a personal webcam or microphone as a fallback. A person may explicitly choose and save another input.
- Video/audio are previewed locally. The product has no recording, streaming, account, analytics upload or automatic diagnostic-upload feature. Normal preview does not require an internet connection. Building downloads dependencies from their public upstreams.
- Local data is stored under `%LOCALAPPDATA%\SwitchCaptureViewer`: settings may include device moniker IDs and audio endpoint IDs; diagnostic logs/status can include device names, identifiers, error details, local paths and timestamps. Optional command-line inspection/self-test creates additional local diagnostic files. These files are not automatically shared.
- Do not attach raw settings, logs, manifests, screenshots or audio/video to a public issue. First inspect and redact them. Prefer a short written reproduction using non-sensitive device model names.
- The public source/release staging excludes the tester's settings, logs, hardware instance identifiers, private captures, local shortcut and debug symbols. Public author attribution uses the GitHub account and a GitHub noreply identity, not a personal email.
- Keep your own local recordings/screenshots and credentials outside this repository. `.gitignore` is a guard against accidental addition, not a secret scanner or access-control mechanism.

## ภาษาไทย

- แอปใช้การ์ดที่เลือกและจำไว้ หรือ Live Gamer MINI ที่ระบุได้ในการใช้ครั้งแรก ไม่เดาเลือกเว็บแคมหรือไมโครโฟนส่วนตัวแทน ผู้ใช้ยังเลือกอุปกรณ์อื่นเองและบันทึกได้
- ภาพและเสียงเป็นพรีวิวในเครื่อง ไม่มีฟีเจอร์บันทึก สตรีม บัญชีผู้ใช้ analytics upload หรืออัปโหลด diagnostic อัตโนมัติ การดูภาพปกติไม่ต้องต่ออินเทอร์เน็ต ส่วนการ build ดาวน์โหลด dependency จาก upstream สาธารณะ
- ข้อมูลในเครื่องอยู่ที่ `%LOCALAPPDATA%\SwitchCaptureViewer` ค่าที่จำไว้อาจมี moniker ID/endpoint ID ส่วน log/status อาจมีชื่อ/ตัวระบุอุปกรณ์ รายละเอียด error, path และเวลา CLI inspect/self-test อาจสร้าง diagnostic เพิ่ม ไฟล์เหล่านี้ไม่ถูกแชร์อัตโนมัติ
- อย่าแนบ settings, log, manifest, ภาพหน้าจอหรือเสียง/วิดีโอดิบใน issue สาธารณะ ตรวจและลบข้อมูลส่วนตัวก่อน ควรเริ่มจากขั้นตอนทำซ้ำเป็นข้อความและชื่อรุ่นอุปกรณ์ที่ไม่อ่อนไหว
- staging สำหรับ public ตัดค่าของผู้ทดสอบ log ตัวระบุฮาร์ดแวร์เฉพาะเครื่อง ไฟล์แคปเจอร์ shortcut และ debug symbols ออก ใช้ชื่อบัญชี GitHub กับ noreply สำหรับ attribution ไม่ใช่อีเมลส่วนตัว
- เก็บไฟล์ส่วนตัวและ credential นอก repo `.gitignore` ช่วยป้องกันการ add โดยเผลอ แต่ไม่ใช่ระบบสแกน secret หรือควบคุมสิทธิ์เข้าถึง
