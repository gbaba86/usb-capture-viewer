# Testing and limitations / การทดสอบและข้อจำกัด

## English

The local baseline was exercised with Nintendo Switch 2 through its powered dock into AVerMedia Live Gamer MINI GC311 and USB to a Windows x64 PC. Video input and matching HDMI audio were selected separately. Real gameplay appeared in this app, not only a synthetic fixture or a card's No Signal image. Private captures are intentionally not published.

The tested settings are 1920×1080, 60 FPS, MJPG and 50 ms live video cache. Live audio uses independent WASAPI shared capture/output through NAudio. An earlier shared DirectShow A/V clock caused late-audio drops at low cache; increasing the video cache made delay worse. The accepted baseline instead separates live audio from video and bounds its queues.

The tester reported that video/audio became playable, with some remaining perceptible delay. Repeated Switch 2 undock/redock cycles were confirmed working by the tester. This does not certify every HDMI/USB hotplug sequence or capture-card model.

A sampled 10.05-second counter interval displayed 601 pictures (about 59.8 FPS), consumed about 3.86 MB of audio, and counted about 0.1% missing audio bytes at the software-buffer boundary. No stale-audio overflow discard or audio warning was recorded in that interval. A process-specific Windows audio-session meter also showed non-zero output. These are throughput/buffer observations, **not** a guarantee of perfect audio, measured end-to-end latency, or input-to-photon timing.

Earlier local synthetic lifecycle checks covered settings, menu state, volume/mute, fullscreen, decoding, stop/restart and resource release. Icon checks verified all nine native EXE icon frames and the Form resource. The public build removes debug symbols and maps source paths; capture behavior has not been deliberately changed for publication, and no new hardware session was started for this packaging work.

Remaining limitations include long-duration A/V drift, all Windows output/device changes, duplicate-friendly-name devices, USB hotplug variations and untested consoles/cards. Display is 16:9 SDR, with common capture formats up to 1080p60; legacy aspect ratios, HDR/4K/high-refresh capture are outside this preview's scope. A card emitting black/No Signal frames can require manual reconnect. No precise total latency or zero-latency claim is made. The binary is unsigned.

## ภาษาไทย

ทดสอบ baseline ในเครื่องกับ Nintendo Switch 2 ในด็อกที่จ่ายไฟ ต่อ HDMI เข้า AVerMedia Live Gamer MINI GC311 แล้ว USB เข้าคอม Windows x64 เลือกวิดีโอและเสียง HDMI แยกกัน เห็น gameplay จริงในแอป ไม่ใช่เฉพาะ synthetic clip หรือภาพ No Signal ภาพ/เสียงส่วนตัวไม่ได้เผยแพร่

ค่าที่ทดสอบคือ 1920×1080, 60 FPS, MJPG และบัฟเฟอร์ภาพ 50 ms เสียงใช้ WASAPI shared แยกผ่าน NAudio รุ่นก่อนที่ใช้ clock DirectShow ร่วมกันเกิดการทิ้งเสียงช้าเมื่อ cache ต่ำ การเพิ่มบัฟเฟอร์ภาพทำให้หน่วงขึ้น baseline ที่ยอมรับจึงแยกทางเสียงและจำกัดคิว

ผู้ทดสอบยืนยันว่าภาพและเสียงดีขึ้นจนพอเล่นได้ แต่ยังสัมผัสความหน่วง และยืนยันยก Switch 2 ออกจากด็อกแล้ววางกลับหลายรอบผ่าน ไม่ถือว่ารับรอง HDMI/USB hotplug ทุกแบบหรือทุกการ์ด

ตัวอย่างช่วงวัด 10.05 วินาทีแสดง 601 ภาพ ประมาณ 59.8 FPS เล่นข้อมูลเสียงประมาณ 3.86 MB และนับข้อมูลขาดที่ขอบบัฟเฟอร์ซอฟต์แวร์ประมาณ 0.1% ไม่พบการทิ้งเสียงเพราะคิวเก่าล้นหรือ audio warning ในช่วงนั้น มิเตอร์เสียง Windows เฉพาะ process พบสัญญาณออกจริง เป็นการดู throughput/บัฟเฟอร์ **ไม่ใช่** การรับรองว่าเสียงสมบูรณ์แบบ หรือการวัด latency รวม/input-to-photon

การตรวจ synthetic ในเครื่องก่อนหน้าครอบคลุมการจำค่า เมนู volume/mute เต็มจอ ถอดรหัส หยุด/เริ่มซ้ำและคืน resource ตรวจไอคอน native EXE ครบเก้าขนาดและ resource ของ Form ชุด public ตัด debug symbols และใช้ source path กลาง ไม่ได้ตั้งใจเปลี่ยนพฤติกรรมจับภาพเพื่อเผยแพร่ และไม่ได้เปิด hardware session ใหม่ในงานแพ็กเกจนี้

ยังไม่ทดสอบ A/V drift หลายชั่วโมง การสลับอุปกรณ์/ปลายทางเสียง Windows ทุกแบบ อุปกรณ์ชื่อซ้ำ USB hotplug ทุกแบบและการ์ด/เครื่องเกมอื่นทั้งหมด พรีวิวเป็น 16:9 SDR โหมดทั่วไปไม่เกิน 1080p60 ไม่ครอบคลุมอัตราส่วนภาพเก่า HDR/4K/refresh สูง หากการ์ดส่งเฟรมดำ/No Signal อาจต้องเชื่อมต่อใหม่เอง ไม่อ้าง latency รวมที่แน่นอนหรือ zero latency ไบนารีไม่ได้ลงนามดิจิทัล


Hardware feedback refers to the earlier local baseline; the public .NET 10 runtime upgrade was rechecked with synthetic video/audio, without a new hardware capture session.
