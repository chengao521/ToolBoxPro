using System.IO;
using System.Net;
using System.Text;
using ToolBoxPro.Core.Interfaces;
using ToolBoxPro.Core.Models;

namespace ToolBoxPro.Services
{
    public class ProteinViewerService : IProteinViewerService
    {
        public async Task<OperationResult<string>> GenerateViewerAsync(ProteinViewerOptions options)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(options.OutputPath))
                {
                    return OperationResult<string>.Fail("请选择输出文件路径");
                }

                var directory = Path.GetDirectoryName(options.OutputPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var html = GenerateHtml(options);
                await File.WriteAllTextAsync(options.OutputPath, html, Encoding.UTF8);

                return OperationResult<string>.Ok(options.OutputPath, "蛋白质 3D 查看器生成成功");
            }
            catch (Exception ex)
            {
                return OperationResult<string>.Fail($"生成蛋白质 3D 查看器失败: {ex.Message}", ex);
            }
        }

        public string GenerateHtml(ProteinViewerOptions options)
        {
            var title = WebUtility.HtmlEncode(options.Title);
            var pdbUrl = EscapeJavaScript(options.PdbFileUrl);
            var helixColor = NormalizeColor(options.HelixColor, "#FF00FF");
            var sheetColor = NormalizeColor(options.SheetColor, "#FFD700");
            var coilColor = NormalizeColor(options.CoilColor, "#808080");
            var bgColor = NormalizeColor(options.BackgroundColor, "#0a0e1a");
            var showBall = options.ShowBallAndStick ? "true" : "false";
            var showRibbon = options.ShowRibbon ? "true" : "false";
            var showLabels = options.ShowLabels ? "true" : "false";
            var atomRadius = Math.Max(1, options.AtomRadiusScale);
            var ribbonWidth = Math.Max(1, options.RibbonWidth);

            return $$"""
<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>{{title}}</title>
  <style>
    * { box-sizing: border-box; }
    body {
      margin: 0;
      overflow: hidden;
      background: {{bgColor}};
      font-family: "Microsoft YaHei", Arial, sans-serif;
      color: #fff;
    }
    #container {
      width: 100vw;
      height: 100vh;
      position: relative;
    }
    #title {
      position: fixed;
      top: 24px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 10;
      padding: 10px 28px;
      border: 1px solid rgba(0, 200, 255, 0.4);
      border-radius: 999px;
      background: rgba(2, 16, 32, 0.7);
      box-shadow: 0 0 30px rgba(0, 200, 255, 0.2);
      letter-spacing: 4px;
      font-size: 22px;
      font-weight: 700;
      white-space: nowrap;
    }
    #tooltip {
      position: fixed;
      display: none;
      z-index: 20;
      pointer-events: none;
      padding: 8px 14px;
      border: 1px solid rgba(255, 255, 255, 0.45);
      border-radius: 6px;
      background: rgba(0, 0, 0, 0.78);
      color: #fff;
      font-size: 14px;
      box-shadow: 0 0 16px rgba(0, 200, 255, 0.35);
      max-width: 300px;
    }
    #panel {
      position: fixed;
      left: 24px;
      bottom: 24px;
      z-index: 10;
      max-width: 400px;
      padding: 14px 18px;
      border: 1px solid rgba(0, 200, 255, 0.28);
      border-radius: 12px;
      background: rgba(2, 16, 32, 0.65);
      line-height: 1.8;
      color: rgba(255, 255, 255, 0.84);
      font-size: 13px;
    }
    #panel h4 {
      margin: 0 0 8px 0;
      font-size: 15px;
      color: #00c8ff;
    }
    #stats {
      position: fixed;
      right: 24px;
      bottom: 24px;
      z-index: 10;
      padding: 10px 16px;
      border: 1px solid rgba(0, 200, 255, 0.28);
      border-radius: 10px;
      background: rgba(2, 16, 32, 0.65);
      font-size: 12px;
      color: rgba(255, 255, 255, 0.7);
      line-height: 1.7;
    }
    #legend {
      position: fixed;
      left: 24px;
      top: 100px;
      z-index: 10;
      padding: 12px 16px;
      border: 1px solid rgba(0, 200, 255, 0.28);
      border-radius: 10px;
      background: rgba(2, 16, 32, 0.65);
      font-size: 12px;
      line-height: 2;
    }
    .legend-item { display: flex; align-items: center; gap: 8px; }
    .legend-dot { width: 12px; height: 12px; border-radius: 50%; display: inline-block; }
    #error {
      position: fixed;
      left: 50%;
      top: 50%;
      transform: translate(-50%, -50%);
      display: none;
      max-width: 760px;
      padding: 22px 26px;
      border: 1px solid rgba(255, 80, 80, 0.7);
      border-radius: 12px;
      background: rgba(30, 0, 0, 0.78);
      color: #ffd4d4;
      z-index: 30;
      line-height: 1.7;
    }
    #loading {
      position: fixed;
      left: 50%;
      top: 50%;
      transform: translate(-50%, -50%);
      z-index: 25;
      font-size: 18px;
      color: #00c8ff;
    }
    #loading::after {
      content: "";
      display: block;
      width: 40px;
      height: 40px;
      margin: 16px auto 0;
      border: 3px solid rgba(0, 200, 255, 0.2);
      border-top-color: #00c8ff;
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    #modelToggle {
      position: fixed;
      right: 24px;
      top: 24px;
      z-index: 10;
      padding: 10px 14px;
      border: 1px solid rgba(0, 200, 255, 0.28);
      border-radius: 10px;
      background: rgba(2, 16, 32, 0.65);
      font-size: 13px;
      line-height: 2;
    }
    #modelToggle label { cursor: pointer; display: flex; align-items: center; gap: 6px; }
  </style>
</head>
<body>
  <div id="container"></div>
  <div id="title">{{title}}</div>
  <div id="tooltip"></div>
  <div id="legend">
    <div style="font-weight:bold; margin-bottom:4px; color:#00c8ff;">图例</div>
    <div class="legend-item"><span class="legend-dot" style="background:#FF00FF"></span> α-螺旋 (Helix)</div>
    <div class="legend-item"><span class="legend-dot" style="background:#FFD700"></span> β-折叠 (Sheet)</div>
    <div class="legend-item"><span class="legend-dot" style="background:#808080"></span> 无规卷曲 (Coil)</div>
    <div class="legend-item"><span class="legend-dot" style="background:#C0C0C0"></span> 碳 (C)</div>
    <div class="legend-item"><span class="legend-dot" style="background:#4444ff"></span> 氮 (N)</div>
    <div class="legend-item"><span class="legend-dot" style="background:#ff2222"></span> 氧 (O)</div>
    <div class="legend-item"><span class="legend-dot" style="background:#ffff55"></span> 硫 (S)</div>
  </div>
  <div id="panel">
    <h4>蛋白质 3D 结构查看器</h4>
    鼠标左键拖拽：旋转视角<br />
    鼠标滚轮：缩放视角<br />
    鼠标右键：平移视角<br />
    鼠标悬停：显示原子信息<br />
    数据源：<span id="sourceText"></span>
  </div>
  <div id="modelToggle">
    <label><input type="checkbox" id="chkBall" checked="true" onchange="toggleBallStick(this.checked)" /> 球棍模型</label>
    <label><input type="checkbox" id="chkRibbon" checked="true" onchange="toggleRibbon(this.checked)" /> 彩带模型</label>
    <label><input type="checkbox" id="chkLabels" checked="false" onchange="toggleLabels(this.checked)" /> 残基标签</label>
  </div>
  <div id="stats"></div>
  <div id="error"></div>
  <div id="loading">正在加载蛋白质结构…</div>

  <script src="https://cdn.jsdelivr.net/npm/three@0.132.2/build/three.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/three@0.132.2/examples/js/controls/OrbitControls.js"></script>
  <script>
    const config = {
      pdbUrl: '{{pdbUrl}}',
      showBallAndStick: {{showBall}},
      showRibbon: {{showRibbon}},
      showLabels: {{showLabels}},
      helixColor: '{{helixColor}}',
      sheetColor: '{{sheetColor}}',
      coilColor: '{{coilColor}}',
      backgroundColor: '{{bgColor}}',
      atomRadiusScale: {{atomRadius}},
      ribbonWidth: {{ribbonWidth}}
    };

    document.getElementById('sourceText').textContent = config.pdbUrl;

    const container = document.getElementById('container');
    const tooltip = document.getElementById('tooltip');
    const errorBox = document.getElementById('error');
    const statsBox = document.getElementById('stats');
    const loadingBox = document.getElementById('loading');

    // ============ Three.js setup ============
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(config.backgroundColor);

    const camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 100000);
    camera.position.set(0, 0, 200);

    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setPixelRatio(window.devicePixelRatio || 1);
    renderer.setSize(window.innerWidth, window.innerHeight);
    container.appendChild(renderer.domElement);

    const controls = new THREE.OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.08;
    controls.rotateSpeed = 0.6;
    controls.zoomSpeed = 0.8;
    controls.panSpeed = 0.5;

    // Lighting
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.4);
    scene.add(ambientLight);
    const dirLight1 = new THREE.DirectionalLight(0xffffff, 0.7);
    dirLight1.position.set(1, 1, 1);
    scene.add(dirLight1);
    const dirLight2 = new THREE.DirectionalLight(0xffffff, 0.3);
    dirLight2.position.set(-1, -0.5, -1);
    scene.add(dirLight2);
    const pointLight = new THREE.PointLight(0x00c8ff, 0.3, 500);
    scene.add(pointLight);

    const raycaster = new THREE.Raycaster();
    const mouse = new THREE.Vector2();
    let hoveredObject = null;

    // ============ Element styles (CPK colors) ============
    const ELEMENT_STYLE = {
      'C':  { color: 0x808080, radius: 0.75, vdRadius: 1.70 },
      'N':  { color: 0x4444ff, radius: 0.70, vdRadius: 1.55 },
      'O':  { color: 0xff2222, radius: 0.70, vdRadius: 1.52 },
      'S':  { color: 0xffff55, radius: 1.00, vdRadius: 1.80 },
      'H':  { color: 0xffffff, radius: 0.30, vdRadius: 1.10 },
      'P':  { color: 0xff8000, radius: 1.00, vdRadius: 1.80 },
      'FE': { color: 0xbcc8ff, radius: 1.20, vdRadius: 1.46 },
      'ZN': { color: 0x7d80bf, radius: 1.20, vdRadius: 1.39 },
      'MG': { color: 0x8aff00, radius: 1.20, vdRadius: 1.73 },
      'NA': { color: 0x4489ff, radius: 1.50, vdRadius: 1.65 },
      'CL': { color: 0x1ff01f, radius: 1.50, vdRadius: 1.81 }
    };
    const FALLBACK_STYLE = { color: 0xaaaaaa, radius: 0.75, vdRadius: 1.70 };

    // Covalent radii for bond detection
    const COVALENT_RADIUS = {
      'C': 0.76, 'N': 0.71, 'O': 0.66, 'S': 1.05, 'H': 0.31,
      'P': 1.07, 'FE': 1.32, 'ZN': 1.22, 'MG': 1.30, 'NA': 1.66, 'CL': 1.02
    };
    const FALLBACK_RADIUS = 0.75;
    const BOND_SLACK = 1.3;
    const MIN_BOND_DIST = 0.5;

    // ============ PDB parsing ============
    function parsePDB(text) {
      const lines = text.split('\n');
      const atoms = [];
      const helixes = [];
      const sheets = [];
      const residues = [];
      const chains = [];
      const residueMap = {}; // chainId_resSeq -> residue
      const chainMap = {};
      let atomIdx = 0;
      let resIdx = 0;
      let chainIdx = 0;

      for (const line of lines) {
        if (line.length < 10) continue;

        if (line.substring(0, 6) === 'ATOM  ' || line.substring(0, 6) === 'HETATM') {
          const serial = parseInt(line.substring(6, 11), 10);
          const name = line.substring(12, 16).trim();
          const resName = line.substring(17, 20).trim();
          const chainId = line.substring(21, 22).trim();
          const resSeq = parseInt(line.substring(22, 26), 10);
          const x = parseFloat(line.substring(30, 38));
          const y = parseFloat(line.substring(39, 46));
          const z = parseFloat(line.substring(47, 54));

          // Determine element
          let element;
          if (name.length >= 2 && name[0] === name[0].toUpperCase() && name[1] === name[1].toLowerCase()) {
            element = name;
          } else {
            element = name.substring(0, 1);
          }
          // Special cases
          if (element === ' ' && name.length >= 2) element = name.substring(0, 2);

          const chainKey = chainId || 'X';
          if (!chainMap[chainKey]) {
            chainMap[chainKey] = { id: chainKey, index: chainIdx++ };
            chains.push(chainMap[chainKey]);
          }
          const resKey = chainKey + '_' + resSeq;
          if (!residueMap[resKey]) {
            residueMap[resKey] = { name: resName, seq: resSeq, chainKey: chainKey, chainIndex: chainMap[chainKey].index, index: resIdx++ };
            residues.push(residueMap[resKey]);
          }

          atoms.push({
            index: atomIdx++,
            serial: serial || atomIdx,
            name: name,
            element: element,
            position: [x, y, z],
            residueIndex: residueMap[resKey].index,
            chainIndex: chainMap[chainKey].index
          });
        }
        else if (line.substring(0, 6) === 'HELIX ') {
          const helix = {
            id: line.substring(7, 10).trim(),
            resBegin: line.substring(11, 14).trim(),
            chainBegin: line.substring(15, 16).trim(),
            resBeginNum: parseInt(line.substring(21, 26), 10),
            resEnd: line.substring(27, 30).trim(),
            chainEnd: line.substring(31, 32).trim(),
            resEndNum: parseInt(line.substring(33, 38), 10),
            type: line.substring(38, 39).trim()
          };
          helixes.push(helix);
        }
        else if (line.substring(0, 6) === 'SHEET ') {
          const sheet = {
            id: line.substring(11, 12).trim(),
            resBegin: line.substring(21, 24).trim(),
            chainBegin: line.substring(25, 26).trim(),
            resBeginNum: parseInt(line.substring(27, 32), 10),
            resEnd: line.substring(33, 36).trim(),
            chainEnd: line.substring(37, 38).trim(),
            resEndNum: parseInt(line.substring(39, 44), 10)
          };
          sheets.push(sheet);
        }
      }

      return { atoms, helixes, sheets, residues, chains };
    }

    // ============ Build spatial grid for bond detection ============
    function buildBonds(atoms) {
      const bonds = [];
      const CELL_SIZE = 3.0;
      const grid = new Map();

      for (const a of atoms) {
        const [ax, ay, az] = a.position;
        const cx = Math.floor(ax / CELL_SIZE);
        const cy = Math.floor(ay / CELL_SIZE);
        const cz = Math.floor(az / CELL_SIZE);
        const key = cx + ',' + cy + ',' + cz;
        if (!grid.has(key)) grid.set(key, []);
        grid.get(key).push(a.index);
      }

      for (const a of atoms) {
        const rA = COVALENT_RADIUS[a.element] ?? FALLBACK_RADIUS;
        const [ax, ay, az] = a.position;
        const cx = Math.floor(ax / CELL_SIZE);
        const cy = Math.floor(ay / CELL_SIZE);
        const cz = Math.floor(az / CELL_SIZE);

        for (let dx = -1; dx <= 1; dx++) {
          for (let dy = -1; dy <= 1; dy++) {
            for (let dz = -1; dz <= 1; dz++) {
              const key = (cx + dx) + ',' + (cy + dy) + ',' + (cz + dz);
              const bucket = grid.get(key);
              if (!bucket) continue;
              for (const j of bucket) {
                if (j <= a.index) continue;
                const b = atoms[j];
                const rB = COVALENT_RADIUS[b.element] ?? FALLBACK_RADIUS;
                const cutoff = (rA + rB) * BOND_SLACK;
                const ddx = b.position[0] - ax;
                const ddy = b.position[1] - ay;
                const ddz = b.position[2] - az;
                const distSq = ddx * ddx + ddy * ddy + ddz * ddz;
                if (distSq > MIN_BOND_DIST * MIN_BOND_DIST && distSq < cutoff * cutoff) {
                  bonds.push([a.index, j]);
                }
              }
            }
          }
        }
      }
      return bonds;
    }

    // ============ Create ball-and-stick model ============
    function createBallAndStick(atoms, bonds) {
      const group = new THREE.Group();
      group.name = 'ballAndStick';

      // Compute center
      let cx = 0, cy = 0, cz = 0;
      for (const a of atoms) {
        cx += a.position[0];
        cy += a.position[1];
        cz += a.position[2];
      }
      const count = atoms.length;
      if (count > 0) {
        cx /= count; cy /= count; cz /= count;
      }

      // --- Atom spheres (InstancedMesh) ---
      const atomGeo = new THREE.SphereGeometry(1, 10, 10);
      const atomMat = new THREE.MeshStandardMaterial({ roughness: 0.4, metalness: 0.1 });
      const atomMesh = new THREE.InstancedMesh(atomGeo, atomMat, count);
      atomMesh.frustumCulled = false;
      atomMesh.name = 'atomMesh';

      const dummy = new THREE.Object3D();
      const color = new THREE.Color();
      for (let i = 0; i < count; i++) {
        const atom = atoms[i];
        const style = ELEMENT_STYLE[atom.element] ?? FALLBACK_STYLE;
        const radius = style.radius * (config.atomRadiusScale / 10);
        dummy.position.set(atom.position[0] - cx, atom.position[1] - cy, atom.position[2] - cz);
        dummy.scale.setScalar(radius);
        dummy.updateMatrix();
        atomMesh.setMatrixAt(i, dummy.matrix);
        atomMesh.setColorAt(i, color.set(style.color));
        atomMesh.userData['atom_' + i] = atom;
      }
      atomMesh.instanceMatrix.needsUpdate = true;
      if (atomMesh.instanceColor) atomMesh.instanceColor.needsUpdate = true;
      group.add(atomMesh);

      // --- Bond sticks (InstancedMesh with 2 cylinders per bond) ---
      const bondCount = bonds.length * 2;
      if (bondCount > 0) {
        const bondGeo = new THREE.CylinderGeometry(1, 1, 1, 8, 1, true);
        const bondMat = new THREE.MeshStandardMaterial({ roughness: 0.4, metalness: 0.1 });
        const bondMesh = new THREE.InstancedMesh(bondGeo, bondMat, bondCount);
        bondMesh.frustumCulled = false;
        bondMesh.name = 'bondMesh';

        const Y_AXIS = new THREE.Vector3(0, 1, 0);
        const dir = new THREE.Vector3();
        const quat = new THREE.Quaternion();
        const STICK_RADIUS = 0.15 * (config.atomRadiusScale / 10);

        bonds.forEach(([ia, ib], bi) => {
          const a = atoms[ia];
          const b = atoms[ib];
          const ax = a.position[0] - cx;
          const ay = a.position[1] - cy;
          const az = a.position[2] - cz;
          const bx = b.position[0] - cx;
          const by = b.position[1] - cy;
          const bz = b.position[2] - cz;

          dir.set(bx - ax, by - ay, bz - az);
          const len = dir.length();
          if (len < 0.01) return;
          dir.normalize();
          quat.setFromUnitVectors(Y_AXIS, dir);

          // Half toward a
          dummy.quaternion.copy(quat);
          dummy.scale.set(STICK_RADIUS, len / 2, STICK_RADIUS);
          dummy.position.set(ax + (dir.x * len) / 4, ay + (dir.y * len) / 4, az + (dir.z * len) / 4);
          dummy.updateMatrix();
          bondMesh.setMatrixAt(bi * 2, dummy.matrix);
          const styleA = ELEMENT_STYLE[a.element] ?? FALLBACK_STYLE;
          bondMesh.setColorAt(bi * 2, color.set(styleA.color));

          // Half toward b
          dummy.position.set(bx - (dir.x * len) / 4, by - (dir.y * len) / 4, bz - (dir.z * len) / 4);
          dummy.updateMatrix();
          bondMesh.setMatrixAt(bi * 2 + 1, dummy.matrix);
          const styleB = ELEMENT_STYLE[b.element] ?? FALLBACK_STYLE;
          bondMesh.setColorAt(bi * 2 + 1, color.set(styleB.color));
        });

        bondMesh.instanceMatrix.needsUpdate = true;
        if (bondMesh.instanceColor) bondMesh.instanceColor.needsUpdate = true;
        group.add(bondMesh);
      }

      group.userData.center = [cx, cy, cz];
      return group;
    }

    // ============ Determine secondary structure per residue ============
    function getResidueSecondaryStructure(residues, helixes, sheets, chains) {
      // Map chainId to chain object
      const chainIdMap = {};
      for (const c of chains) chainIdMap[c.id] = c;

      const structMap = new Map(); // residueIndex -> 'helix' | 'sheet' | 'coil'

      // Mark helix residues
      for (const h of helixes) {
        const chainKey = h.chainBegin || h.chainEnd || 'A';
        for (let s = h.resBeginNum; s <= h.resEndNum; s++) {
          const key = chainKey + '_' + s;
          const res = residues.find(r => r.name === h.resBegin || r.name === h.resEnd || (r.chainKey === chainKey && r.seq === s));
          // Try to find by chain + seq
          const matchRes = residues.find(r => r.chainKey === chainKey && r.seq === s);
          if (matchRes) structMap.set(matchRes.index, 'helix');
        }
      }

      // Mark sheet residues
      for (const sh of sheets) {
        const chainKey = sh.chainBegin || sh.chainEnd || 'A';
        for (let s = sh.resBeginNum; s <= sh.resEndNum; s++) {
          const matchRes = residues.find(r => r.chainKey === chainKey && r.seq === s);
          if (matchRes) structMap.set(matchRes.index, 'sheet');
        }
      }

      return structMap;
    }

    // ============ Create ribbon/cartoon model ============
    function createRibbon(model, structMap) {
      const group = new THREE.Group();
      group.name = 'ribbon';

      const { atoms, residues, chains } = model;
      const structColorMap = {
        'helix': new THREE.Color(config.helixColor),
        'sheet': new THREE.Color(config.sheetColor),
        'coil': new THREE.Color(config.coilColor)
      };

      // Group CA atoms by chain
      const chainCA = new Map(); // chainIndex -> [atomIndex, ...]
      for (const atom of atoms) {
        if (atom.name.toUpperCase() === 'CA') {
          if (!chainCA.has(atom.chainIndex)) chainCA.set(atom.chainIndex, []);
          chainCA.get(atom.chainIndex).push(atom.index);
        }
      }

      // Compute center
      let cx = 0, cy = 0, cz = 0;
      let totalCA = 0;
      for (const [chainIdx, caAtoms] of chainCA) {
        for (const ai of caAtoms) {
          cx += atoms[ai].position[0];
          cy += atoms[ai].position[1];
          cz += atoms[ai].position[2];
          totalCA++;
        }
      }
      if (totalCA > 0) { cx /= totalCA; cy /= totalCA; cz /= totalCA; }

      const ribbonWidth = config.ribbonWidth;
      const positions = [];
      const normals = [];
      const colors = [];
      const indices = [];

      // Build ribbon per chain
      for (const [chainIdx, caAtoms] of chainCA) {
        if (caAtoms.length < 2) continue;

        // Sort by serial number
        caAtoms.sort((a, b) => atoms[a].serial - atoms[b].serial);

        // Get CA positions centered
        const caPositions = caAtoms.map(ai => {
          const a = atoms[ai];
          return [a.position[0] - cx, a.position[1] - cy, a.position[2] - cz];
        });

        // Build ribbon vertices: for each CA, create a cross-section ring
        // Width direction: perpendicular to chain direction, toward O atom direction
        // Thickness: perpendicular to both
        const ringCount = 8; // vertices per ring
        const ringPositions = [];

        for (let i = 0; i < caPositions.length; i++) {
          const p = caPositions[i];
          // Tangent direction
          let tangent;
          if (i === 0) {
            tangent = [caPositions[1][0] - caPositions[0][0], caPositions[1][1] - caPositions[0][1], caPositions[1][2] - caPositions[0][2]];
          } else if (i === caPositions.length - 1) {
            tangent = [caPositions[i][0] - caPositions[i-1][0], caPositions[i][1] - caPositions[i-1][1], caPositions[i][2] - caPositions[i-1][2]];
          } else {
            tangent = [caPositions[i+1][0] - caPositions[i-1][0], caPositions[i+1][1] - caPositions[i-1][1], caPositions[i+1][2] - caPositions[i-1][2]];
          }
          const tLen = Math.sqrt(tangent[0]*tangent[0] + tangent[1]*tangent[1] + tangent[2]*tangent[2]);
          if (tLen < 0.001) tangent = [1, 0, 0];
          tangent = [tangent[0]/tLen, tangent[1]/tLen, tangent[2]/tLen];

          // Determine width direction from CA->O if available
          const atom = atoms[caAtoms[i]];
          let widthDir;
          // Find O atom in same residue
          const oAtom = atoms.find(a2 => a2.residueIndex === atom.residueIndex && (a2.name === 'O' || a2.name === 'O1'));
          if (oAtom) {
            const ox = oAtom.position[0] - cx - p[0];
            const oy = oAtom.position[1] - cy - p[1];
            const oz = oAtom.position[2] - cz - p[2];
            // Cross product of tangent and CA->O to get perpendicular
            widthDir = [
              tangent[1]*oz - tangent[2]*oy,
              tangent[2]*ox - tangent[0]*oz,
              tangent[0]*oy - tangent[1]*ox
            ];
          } else {
            // Fallback: use up vector
            widthDir = [0, 1, 0];
            // Make perpendicular to tangent
            const dot = widthDir[0]*tangent[0] + widthDir[1]*tangent[1] + widthDir[2]*tangent[2];
            widthDir = [widthDir[0] - dot*tangent[0], widthDir[1] - dot*tangent[1], widthDir[2] - dot*tangent[2]];
          }
          const wLen = Math.sqrt(widthDir[0]*widthDir[0] + widthDir[1]*widthDir[1] + widthDir[2]*widthDir[2]);
          if (wLen < 0.001) widthDir = [1, 0, 0];
          widthDir = [widthDir[0]/wLen, widthDir[1]/wLen, widthDir[2]/wLen];

          // Normal direction (thickness)
          const normal = [
            widthDir[1]*tangent[2] - widthDir[2]*tangent[1],
            widthDir[2]*tangent[0] - widthDir[0]*tangent[2],
            widthDir[0]*tangent[1] - widthDir[1]*tangent[0]
          ];

          // Determine secondary structure for this residue
          const residue = residues[atom.residueIndex];
          const struct = structMap.get(residue.index) || 'coil';
          const structColor = structColorMap[struct] || structColorMap['coil'];

          // Adjust width based on structure
          let wScale = 1.0;
          let nScale = 1.0;
          if (struct === 'helix') {
            wScale = 1.8;
            nScale = 0.6;
          } else if (struct === 'sheet') {
            wScale = 2.5;
            nScale = 0.4;
          }

          const hw = ribbonWidth * 0.5 * wScale;
          const hn = ribbonWidth * 0.5 * nScale;

          // Create ring vertices
          for (let r = 0; r < ringCount; r++) {
            const angle = (r / ringCount) * Math.PI * 2;
            const cosA = Math.cos(angle);
            const sinA = Math.sin(angle);
            const vx = p[0] + (widthDir[0] * cosA + normal[0] * sinA) * (hw * Math.abs(cosA) + hn * Math.abs(sinA));
            const vy = p[1] + (widthDir[1] * cosA + normal[1] * sinA) * (hw * Math.abs(cosA) + hn * Math.abs(sinA));
            const vz = p[2] + (widthDir[2] * cosA + normal[2] * sinA) * (hw * Math.abs(cosA) + hn * Math.abs(sinA));

            positions.push(vx, vy, vz);
            // Normal: from center outward
            const nx = vx - p[0];
            const ny = vy - p[1];
            const nz = vz - p[2];
            const nLen = Math.sqrt(nx*nx + ny*ny + nz*nz) || 1;
            normals.push(nx/nLen, ny/nLen, nz/nLen);
            colors.push(structColor.r, structColor.g, structColor.b);
          }
        }

        // Create indices between consecutive rings
        const totalRings = caPositions.length;
        for (let r = 0; r < totalRings - 1; r++) {
          for (let v = 0; v < ringCount; v++) {
            const next = (v + 1) % ringCount;
            const i0 = r * ringCount + v;
            const i1 = r * ringCount + next;
            const i2 = (r + 1) * ringCount + next;
            const i3 = (r + 1) * ringCount + v;
            indices.push(i0, i1, i2);
            indices.push(i0, i2, i3);
          }
        }

        // Add arrow heads for beta sheets at the end
        for (let i = 0; i < caAtoms.length - 1; i++) {
          const residue = residues[atoms[caAtoms[i]].residueIndex];
          const struct = structMap.get(residue.index) || 'coil';
          if (struct === 'sheet' && i < caAtoms.length - 2) {
            // Check if next residue is not sheet
            const nextResidue = residues[atoms[caAtoms[i+1]].residueIndex];
            const nextStruct = structMap.get(nextResidue.index) || 'coil';
            if (nextStruct !== 'sheet') {
              // Add arrow head at position i+1
              const p = caPositions[i + 1];
              const prev = caPositions[i];
              const dirA = [p[0]-prev[0], p[1]-prev[1], p[2]-prev[2]];
              const dLen = Math.sqrt(dirA[0]*dirA[0]+dirA[1]*dirA[1]+dirA[2]*dirA[2]) || 1;
              dirA = [dirA[0]/dLen, dirA[1]/dLen, dirA[2]/dLen];

              // Arrow tip
              const tip = [p[0] + dirA[0]*ribbonWidth*1.5, p[1] + dirA[1]*ribbonWidth*1.5, p[2] + dirA[2]*ribbonWidth*1.5];
              const base1 = [p[0] - dirA[0]*ribbonWidth*0.5 + widthDir0(p, dirA, caPositions, i, ringCount) * ribbonWidth,
                             p[1] - dirA[1]*ribbonWidth*0.5 + (i > 0 ? 0 : 0), 0];
              // Simplified: just add a triangle
              const structColor = structColorMap['sheet'];
              const basePos = caPositions[i];
              const v0 = ringCount * (i + 1);
              // Tip vertex
              positions.push(tip[0], tip[1], tip[2]);
              normals.push(dirA[0], dirA[1], dirA[2]);
              colors.push(structColor.r, structColor.g, structColor.b);
              // Left base vertex
              positions.push(basePos[0], basePos[1], basePos[2]);
              normals.push(0,0,0);
              colors.push(structColor.r, structColor.g, structColor.b);
              // Right base vertex (offset)
              const perp = [dirA[1], -dirA[0], 0];
              const pLen = Math.sqrt(perp[0]*perp[0]+perp[1]*perp[1]) || 1;
              positions.push(basePos[0]+perp[0]/pLen*ribbonWidth, basePos[1]+perp[1]/pLen*ribbonWidth, basePos[2]);
              normals.push(0,0,0);
              colors.push(structColor.r, structColor.g, structColor.b);

              indices.push(positions.length/3-3, positions.length/3-2, positions.length/3-1);
            }
          }
        }
      }

      // Build geometry
      const geometry = new THREE.BufferGeometry();
      geometry.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
      geometry.setAttribute('normal', new THREE.Float32BufferAttribute(normals, 3));
      geometry.setAttribute('color', new THREE.Float32BufferAttribute(colors, 3));
      geometry.setIndex(indices);
      geometry.computeVertexNormals();

      const material = new THREE.MeshStandardMaterial({
        vertexColors: true,
        side: THREE.DoubleSide,
        roughness: 0.35,
        metalness: 0.15
      });

      const ribbonMesh = new THREE.Mesh(geometry, material);
      ribbonMesh.name = 'ribbonMesh';
      group.add(ribbonMesh);
      group.userData.center = [cx, cy, cz];
      return group;
    }

    function widthDir0(p, dir, caPositions, i, ringCount) {
      // Simple width direction
      return [0, 1, 0];
    }

    // ============ Raycasting / hover ============
    function onMouseMove(event) {
      mouse.x = (event.clientX / window.innerWidth) * 2 - 1;
      mouse.y = -(event.clientY / window.innerHeight) * 2 + 1;
      raycaster.setFromCamera(mouse, camera);

      const targets = [];
      const ballGroup = scene.getObjectByName('ballAndStick');
      if (ballGroup && ballGroup.visible) {
        const atomMesh = ballGroup.getObjectByName('atomMesh');
        if (atomMesh) targets.push(atomMesh);
      }

      const intersects = raycaster.intersectObjects(targets, false);

      if (hoveredObject && (!intersects.length || intersects[0].object !== hoveredObject)) {
        hoveredObject = null;
        tooltip.style.display = 'none';
      }

      if (intersects.length) {
        const obj = intersects[0].object;
        if (obj !== hoveredObject) {
          hoveredObject = obj;
          const idx = intersects[0].instanceId;
          if (idx !== undefined) {
            const atom = obj.userData['atom_' + idx];
            if (atom) {
              const residue = window._modelResidues?.[atom.residueIndex];
              tooltip.textContent = atom.element + ' ' + atom.name +
                (residue ? '  |  ' + residue.name + ' #' + residue.seq : '');
              tooltip.style.display = 'block';
            } else {
              tooltip.style.display = 'none';
            }
          }
        }
        tooltip.style.left = event.clientX + 14 + 'px';
        tooltip.style.top = event.clientY + 14 + 'px';
      }
    }

    function onResize() {
      camera.aspect = window.innerWidth / window.innerHeight;
      camera.updateProjectionMatrix();
      renderer.setSize(window.innerWidth, window.innerHeight);
    }

    function toggleBallStick(show) {
      const g = scene.getObjectByName('ballAndStick');
      if (g) g.visible = show;
    }
    function toggleRibbon(show) {
      const g = scene.getObjectByName('ribbon');
      if (g) g.visible = show;
    }
    function toggleLabels() {
      // Placeholder for label toggling
    }

    // ============ Main ============
    async function loadProtein() {
      try {
        const response = await fetch(config.pdbUrl);
        if (!response.ok) throw new Error('PDB 请求失败: ' + response.status + ' ' + response.statusText);
        const text = await response.text();
        if (!text || text.length < 100) throw new Error('PDB 文件内容无效或为空');

        const model = parsePDB(text);
        if (model.atoms.length === 0) throw new Error('未解析到任何 ATOM 记录');

        window._modelResidues = model.residues;

        // Build bonds
        const bonds = buildBonds(model.atoms);

        // Determine secondary structure
        const structMap = getResidueSecondaryStructure(model.residues, model.helixes, model.sheets, model.chains);

        // Create models
        if (config.showBallAndStick) {
          const ballGroup = createBallAndStick(model.atoms, bonds);
          scene.add(ballGroup);
        }
        if (config.showRibbon) {
          const ribbonGroup = createRibbon(model, structMap);
          scene.add(ribbonGroup);
        }

        // Fit to view
        const box = new THREE.Box3().setFromObject(scene);
        const center = box.getCenter(new THREE.Vector3());
        const size = box.getSize(new THREE.Vector3());
        const maxSize = Math.max(size.x, size.y, size.z);
        if (maxSize > 0) {
          camera.position.set(center.x, center.y, center.z + maxSize * 1.8);
          controls.target.copy(center);
          controls.update();
        }

        // Stats
        const helixCount = model.helixes.length;
        const sheetCount = model.sheets.length;
        const atomCount = model.atoms.length;
        statsBox.innerHTML =
          '原子数: ' + atomCount + '<br>' +
          '残基数: ' + model.residues.length + '<br>' +
          '链数: ' + model.chains.length + '<br>' +
          'α-螺旋: ' + helixCount + '<br>' +
          'β-折叠: ' + sheetCount + '<br>' +
          '键数: ' + bonds.length;

        // Show checkboxes state
        document.getElementById('chkBall').checked = config.showBallAndStick;
        document.getElementById('chkRibbon').checked = config.showRibbon;
        document.getElementById('chkLabels').checked = config.showLabels;

        if (!config.showBallAndStick) {
          const g = scene.getObjectByName('ballAndStick');
          if (g) g.visible = false;
        }
        if (!config.showRibbon) {
          const g = scene.getObjectByName('ribbon');
          if (g) g.visible = false;
        }

        loadingBox.style.display = 'none';
      } catch (error) {
        console.error(error);
        loadingBox.style.display = 'none';
        errorBox.style.display = 'block';
        errorBox.innerHTML = '蛋白质结构加载失败: ' + error.message +
          '<br/>请检查 PDB 文件地址是否可访问。';
      }
    }

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('resize', onResize);

    loadProtein();

    function animate() {
      requestAnimationFrame(animate);
      controls.update();
      renderer.render(scene, camera);
    }
    animate();
  </script>
</body>
</html>
""";
        }

        private static string NormalizeColor(string value, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            var color = value.Trim();
            if (!color.StartsWith("#")) color = "#" + color;
            return color.Length == 7 ? color : fallback;
        }

        private static string EscapeJavaScript(string value)
        {
            return value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "").Replace("\n", "");
        }
    }
}
