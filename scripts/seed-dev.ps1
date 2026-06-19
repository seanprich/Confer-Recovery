# Seed the local development MongoDB with an OrgAdmin and a chapter.
# Idempotent — does nothing if an OrgAdmin already exists.
#
# Default credentials:
#   email:    admin@example.com
#   password: admin123
#
# After seeding, log in and call PUT /api/chapters/{id}/sfu to set
# the real LiveKit credentials (the seeder leaves them blank).

$js = @'
var db = db.getSiblingDB("confer_dev");

// MemberRole.OrgAdmin = 4 (C# enum stored as int)
if (db.members.countDocuments({ role: 4 }) > 0) {
    print("OrgAdmin already exists - skipping seed.");
    quit(0);
}

var chapter = db.chapters.insertOne({
    name: "Dev Chapter",
    sfuUrl: "ws://localhost:7880",
    liveKitApiKey: "devkey",
    liveKitApiSecretEncrypted: "",
    adminMemberIds: [],
    status: 0,
    createdAt: new Date()
});

// BCrypt cost-12 hash for "admin123"
var member = db.members.insertOne({
    displayName: "Admin",
    email: "admin@example.com",
    passwordHash: "$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR0Ifxm6",
    chapterId: chapter.insertedId,
    role: 4,
    status: 1,
    createdAt: new Date(),
    lastLoginAt: null,
    consentAcknowledgedAt: null,
    consentVersion: null
});

db.chapters.updateOne(
    { _id: chapter.insertedId },
    { $set: { adminMemberIds: [member.insertedId] } }
);

print("Seeded - chapter: " + chapter.insertedId + ", admin: " + member.insertedId);
'@

# Shell runner — reads credentials from the container's own env vars so the
# Unicode password is never passed through Windows/PowerShell argument handling.
$runner = @'
#!/bin/sh
mongosh --quiet \
  -u "$MONGO_INITDB_ROOT_USERNAME" \
  -p "$MONGO_INITDB_ROOT_PASSWORD" \
  --authenticationDatabase admin \
  /tmp/seed.js
'@

# Write both files with LF line endings (required by sh) and copy into the container.
function Write-Lf($content, $path) {
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($content.Replace("`r`n", "`n"))
    [System.IO.File]::WriteAllBytes($path, $bytes)
}

$tmpJs = [System.IO.Path]::GetTempFileName()
$tmpSh = [System.IO.Path]::GetTempFileName()
Write-Lf $js $tmpJs
Write-Lf $runner $tmpSh

docker cp $tmpJs confer-recovery-mongo-1:/tmp/seed.js
docker cp $tmpSh confer-recovery-mongo-1:/tmp/run-seed.sh
Remove-Item $tmpJs, $tmpSh

docker compose exec mongo sh /tmp/run-seed.sh
