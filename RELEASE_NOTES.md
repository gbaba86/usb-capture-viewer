# v1.0.0-beta.1 — experimental Windows preview

## English

USB Capture Viewer previews console HDMI through a compatible USB capture card with local video/audio. This first public preview includes auto-connect for saved devices or recognized GC311 hardware, separate video/audio selection, remembered settings, a collapsible menu, F11/Esc fullscreen, volume/mute, bounded queues, reconnect backoff, and the original desktop icon.

The public package uses **.NET 10.0.15 LTS**, is self-contained for **Windows x64**, and is **unsigned**. Extract the entire ZIP before opening `SwitchCaptureViewer.exe`. No admin installation, OBS setup, streaming or recording is provided. UI labels are currently Thai; English and Thai guides are included.

Hardware baseline: Nintendo Switch 2 + AVerMedia Live Gamer MINI GC311 at 1080p60 MJPG, SDR, 50 ms configured video cache with independent WASAPI audio. The tester found it playable with some remaining perceived delay and confirmed repeated undock/redock recovery. The .NET 10 upgrade was checked with synthetic video/audio and 19 lifecycle checks, not a new physical capture session. Other devices, long-run A/V drift, every USB hotplug sequence and true input-to-photon latency are not certified. 50 ms is not measured total latency; no zero-latency claim is made.

Before publication, the source/release files were curated to exclude tester captures, settings/logs, hardware instance IDs, personal paths, local shortcuts, credentials and debug symbols. Managed package advisory audit and a local Defender scan were performed; see the accompanying public review report for their scope/results. Checksums do not establish safety or a publisher signature. Do not disable antivirus or run as administrator as a workaround.

Assets:

- **USB-Capture-Viewer-v1.0.0-beta.1-win-x64.zip** — portable app and both guides/licenses.
- **USB-Capture-Viewer-v1.0.0-beta.1-source.zip** — editable application source and icon, locked dependencies and build instructions.
- **USB-Capture-Viewer-v1.0.0-beta.1-corresponding-sources.zip** — upstream VLC/dependency source archives and notices/build provenance.
- **SHA256SUMS.txt** — integrity checksums, filenames only.
- **PUBLIC_REVIEW.json** — sanitized packaging/security review summary.

[English guide](https://github.com/gbaba86/usb-capture-viewer/blob/main/docs/USER_GUIDE.en.md) · [Thai guide](https://github.com/gbaba86/usb-capture-viewer/blob/main/docs/USER_GUIDE.th.md) · [Security](https://github.com/gbaba86/usb-capture-viewer/security)

## ภาษาไทย

USB Capture Viewer ดูภาพและฟังเสียงเครื่องเกมผ่านการ์ด HDMI แบบ USB ในเครื่อง รุ่น public ทดลองแรกมีการเชื่อมต่ออุปกรณ์ที่จำไว้/GC311 ที่ระบุได้เอง เลือกภาพและเสียงแยก จำค่า เมนูพับได้ เต็มจอ F11/Esc, volume/mute, คิวจำกัด การลองเชื่อมต่อแบบเว้นระยะและไอคอนต้นฉบับ

ชุด public รวม **.NET 10.0.15 LTS** สำหรับ **Windows x64** ไม่ต้องติดตั้ง runtime แยก และ **ไม่ได้ลงนามดิจิทัล** แตก ZIP ทั้งหมดก่อนเปิด `SwitchCaptureViewer.exe` ไม่ต้องติดตั้งแบบแอดมินหรือตั้ง OBS ไม่มีสตรีม/บันทึกวิดีโอ UI ยังเป็นภาษาไทย มีคู่มือไทยและอังกฤษ

baseline ฮาร์ดแวร์คือ Switch 2 + GC311, 1080p60 MJPG, SDR, บัฟเฟอร์ภาพที่ตั้งไว้ 50 ms และเสียง WASAPI แยก ผู้ทดสอบยืนยันว่าพอเล่นได้แต่ยังรู้สึกหน่วง และเข้าออกด็อกหลายรอบผ่าน การย้ายมา .NET 10 ตรวจด้วยภาพ/เสียงสังเคราะห์และ lifecycle 19 ข้อ ไม่ได้เปิด capture จริงรอบใหม่ ยังไม่รับรองอุปกรณ์อื่น A/V drift ระยะยาว USB hotplug ทุกแบบหรือ latency จากการกดจอยถึงภาพ ค่าบัฟเฟอร์ 50 ms ไม่ใช่ latency รวม ไม่อ้าง zero latency

ก่อนเผยแพร่ตัดข้อมูลผู้ทดสอบ ภาพ/เสียง settings/log ตัวระบุอุปกรณ์เฉพาะเครื่อง path ส่วนตัว shortcut, credential และ PDB ตรวจ advisory ของ managed packages และสแกน Defender ในเครื่องแล้ว ดูขอบเขต/ผลใน PUBLIC_REVIEW.json ค่า checksum ไม่ยืนยันความปลอดภัยหรือผู้ลงนาม อย่าปิด antivirus หรือรันแอดมินเพื่อแก้การบล็อก

ไฟล์ดาวน์โหลดมีชุดพกพา Windows, source แอปและไอคอน, corresponding-source bundle ของ upstream, SHA256SUMS.txt ที่มีเฉพาะชื่อไฟล์ และรายงานสรุปที่ลบข้อมูลส่วนตัวแล้ว คู่มือทั้งสองภาษาอยู่ใน ZIP และ repository
