# v1.0.0-beta.2 — download and executable naming fix

The ready-to-run Windows ZIP now contains **USB-Capture-Viewer/USBCaptureViewer.exe** and matching apphost/DLL/deps/runtimeconfig files. Download links and both language guides identify the Windows binary explicitly. Source archives remain developer downloads. Video/audio logic, device selection, cache defaults and native dependencies are unchanged.

แก้ชื่อไฟล์พร้อมใช้เป็น **USBCaptureViewer.exe** พร้อมไฟล์ runtime ที่ตรงกัน เพิ่มลิงก์ Windows ZIP ชัดเจนและปรับคู่มือไทย/อังกฤษ ไม่เปลี่ยนการรับภาพ/เสียงหรือค่าเริ่มต้น และไม่เกี่ยวกับแอปรุ่นแปลภาษา

Requires Windows x64. Includes self-contained .NET 10.0.15. Experimental, unsigned; some capture latency remains. Synthetic playback/startup checks cover the renamed app; no new hardware session was opened for this naming change.

Upstream binaries are identical to beta.1. Their verified corresponding-source bundle remains available here:
https://github.com/gbaba86/usb-capture-viewer/releases/download/v1.0.0-beta.1/USB-Capture-Viewer-v1.0.0-beta.1-corresponding-sources.zip

SHA256: a101d6e6bd33b36a0700481bffad7e00e9194ec52be832faa0a375aee4668de7
Retain that bundle and the source ZIP/notices when redistributing as required by the bundled libraries' licenses.
