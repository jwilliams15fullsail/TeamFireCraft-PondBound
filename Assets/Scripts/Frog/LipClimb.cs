using UnityEngine;

// Climbing up a wall into an edge that sticks out above it, like a counter top's lip over the
// cabinet front: the frog can't stick to the underside, so instead it goes around the lip. Out
// past the lip's front edge, up past it, then in onto the top, where it stands.
// FrogClimb asks Plan when the way up is blocked, then moves the frog through the three points.
// Pure geometry (physics queries only), so it can be tested on its own.
public static class LipClimb
{
    const float Step = 0.03f; // how finely (m) it measures how far the lip sticks out

    // pos: the frog's centre. wallNormal: the wall it clings to. Returns false when there's no
    // lip to go around (a real ceiling, a lip deeper than maxDepth or thicker than maxThickness,
    // nowhere to stand on top, or something in the way).
    public static bool Plan(Vector3 pos, Vector3 wallNormal, float radius, float lookAhead, float maxDepth, float maxThickness,
                            float minUnderUpDot, float maxSlopeAngle, int mask, Rigidbody self, Vector3[] points)
    {
        float castRadius = radius * 0.8f;
        Vector3 outward = Vector3.ProjectOnPlane(wallNormal, Vector3.up);
        if (outward.sqrMagnitude < 0.25f)
            return false; // not on a wall (a slope or a top): nothing to go around
        outward.Normalize();

        // 1. Right above: something facing down, the underside of a lip. Checked with the whole
        // body (and its small gap from the wall), so even a lip that only just sticks out counts.
        float body = radius + 0.015f;
        // From a little lower, so it still works when the frog is already pressed up against it.
        if (!Cast(pos + Vector3.down * 0.05f, body, Vector3.up, lookAhead + 0.15f, mask, self, out RaycastHit under))
            return false;
        if (Vector3.Dot(under.normal, Vector3.up) >= minUnderUpDot)
            return false; // a wall or slope ahead: the normal inside-corner turn handles it
        float undersideY = under.point.y;

        // 2. How far it sticks out: step away from the wall until the way up is clear.
        float columnHeight = (undersideY - pos.y) + maxThickness + radius;
        float clear = -1f;
        for (float k = Step; k <= maxDepth + radius; k += Step)
        {
            Vector3 origin = pos + outward * k;
            if (Physics.CheckSphere(origin, castRadius, mask, QueryTriggerInteraction.Ignore))
                return false; // something out there too: no room to go round
            if (!Cast(origin, castRadius, Vector3.up, columnHeight, mask, self, out _))
            {
                clear = k;
                break;
            }
        }
        if (clear < 0f)
            return false; // sticks out too far: it's a ceiling, not a lip

        // 3. The top, just inside the lip's front edge.
        float edge = clear - castRadius;                    // the front edge, measured out from the frog's centre
        float land = edge - radius - 0.05f;                 // where the frog stands: a body's width in from it
        float startY = undersideY + maxThickness + radius + 0.05f;
        Vector3 above = pos + outward * land;
        above.y = startY;
        float probe = radius * 0.9f;
        if (Physics.CheckSphere(above, probe, mask, QueryTriggerInteraction.Ignore))
            return false; // no headroom over the top (a shelf right above)
        if (!Cast(above, probe, Vector3.down, startY - undersideY, mask, self, out RaycastHit top))
            return false;
        if (Vector3.Angle(top.normal, Vector3.up) > maxSlopeAngle)
            return false; // can't stand on it
        float thickness = top.point.y - undersideY;
        if (thickness <= 0f || thickness > maxThickness + 0.02f)
            return false;
        Vector3 landing = above + Vector3.down * top.distance + Vector3.up * (radius - probe + 0.02f);

        // 4. Out, up, in. Check each leg is clear for the whole body.
        Vector3 outPoint = pos + outward * (clear + (probe - castRadius) + 0.02f); // the whole body clears the edge
        Vector3 upPoint = new Vector3(outPoint.x, landing.y + 0.03f, outPoint.z);
        if (Blocked(pos, outPoint, probe, mask, self) || Blocked(outPoint, upPoint, probe, mask, self) || Blocked(upPoint, landing, probe, mask, self))
            return false;
        points[0] = outPoint;
        points[1] = upPoint;
        points[2] = landing;
        return true;
    }

    static bool Cast(Vector3 from, float radius, Vector3 dir, float distance, int mask, Rigidbody self, out RaycastHit hit)
    {
        hit = default;
        if (distance <= 0f)
            return false;
        RaycastHit[] hits = Physics.SphereCastAll(from, radius, dir, distance, mask, QueryTriggerInteraction.Ignore);
        bool found = false;
        foreach (RaycastHit h in hits)
        {
            if (self != null && h.rigidbody == self)
                continue;
            if (h.distance <= 0f && h.point == Vector3.zero)
                continue; // started inside it: not a surface ahead
            if (!found || h.distance < hit.distance)
            {
                hit = h;
                found = true;
            }
        }
        return found;
    }

    static bool Blocked(Vector3 from, Vector3 to, float radius, int mask, Rigidbody self)
    {
        Vector3 path = to - from;
        float length = path.magnitude;
        return length > 0.001f && Cast(from, radius, path / length, length, mask, self, out _);
    }
}
